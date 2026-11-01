using Ollama.Api.Models;
using Refit;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Interface for model management endpoints (show, copy, delete, pull, push, create).
/// </summary>
public interface IModels
{
	/// <summary>
	/// Gets the details of a model: its modelfile, parameters, template and licence.
	/// </summary>
	[Post("/api/show")]
	Task<ShowModelResponse> ShowAsync(
		ShowModelRequest request,
		CancellationToken cancellationToken);


	/// <summary>
	/// Copies an existing model to a new name.
	/// </summary>
	[Post("/api/copy")]
	Task<ModelOperationResponse> CopyAsync(
		CopyModelRequest request,
		CancellationToken cancellationToken);


	/// <summary>
	/// Deletes a model and the data it holds.
	/// </summary>
	/// <returns>A response whose status is <c>success</c>.</returns>
	/// <exception cref="ApiException">The model does not exist, or the server refused.</exception>
	async Task<ModelOperationResponse> DeleteAsync(
		DeleteModelRequest request,
		CancellationToken cancellationToken)
	{
		// Ollama answers a successful delete with an empty body, so there is nothing to deserialise.
		using var response = await DeleteInternalAsync(request, cancellationToken).ConfigureAwait(false);

		return response.IsSuccessStatusCode
			? new ModelOperationResponse { Status = "success" }
			: throw response.Error!;
	}

	/// <summary>
	/// Deletes a model, returning the raw response. Use <see cref="DeleteAsync"/>.
	/// </summary>
	[Delete("/api/delete")]
	Task<IApiResponse> DeleteInternalAsync(
		[Body] DeleteModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a model. Returns the raw response so the corresponding As-stream method can read its newline-delimited progress updates.
	/// </summary>
	[Delete("/api/delete")]
	Task<HttpResponseMessage> DeleteStreamInternalAsync(
		[Body] DeleteModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Whether the server already holds the blob with this digest, so that
	/// <see cref="PushBlobAsync"/> can be skipped.
	/// </summary>
	/// <param name="digest">The blob's SHA-256 digest, as <c>sha256:</c> followed by 64 hex digits.</param>
	/// <param name="cancellationToken">Cancels the call.</param>
	/// <exception cref="HttpRequestException">The digest is malformed, or the server failed.</exception>
	async Task<bool> BlobExistsAsync(string digest, CancellationToken cancellationToken)
	{
		using var response = await HeadBlobInternalAsync(digest, cancellationToken).ConfigureAwait(false);

		if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return false;
		}

		response.EnsureSuccessStatusCode();

		return true;
	}

	/// <summary>
	/// Checks for a blob, returning the raw response. Use <see cref="BlobExistsAsync"/>.
	/// </summary>
	[Head("/api/blobs/{digest}")]
	Task<HttpResponseMessage> HeadBlobInternalAsync(string digest, CancellationToken cancellationToken);

	/// <summary>
	/// Uploads a model file, such as a GGUF, so that <see cref="CreateAsync"/> can name it in
	/// <see cref="CreateModelRequest.Files"/>.
	/// </summary>
	/// <param name="digest">The content's SHA-256 digest, as <c>sha256:</c> followed by 64 hex digits.</param>
	/// <param name="content">The file's bytes. The caller keeps ownership of the stream.</param>
	/// <param name="cancellationToken">Cancels the call.</param>
	/// <exception cref="HttpRequestException">The digest does not match the content, or the server failed.</exception>
	async Task PushBlobAsync(string digest, Stream content, CancellationToken cancellationToken)
	{
		using var body = new LeaveOpenStreamContent(content);
		using var response = await PushBlobInternalAsync(digest, body, cancellationToken).ConfigureAwait(false);

		response.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Uploads a blob, returning the raw response. Use <see cref="PushBlobAsync"/>.
	/// </summary>
	[Post("/api/blobs/{digest}")]
	Task<HttpResponseMessage> PushBlobInternalAsync(
		string digest,
		[Body] HttpContent content,
		CancellationToken cancellationToken);

	/// <summary>
	/// Downloads a model from the Ollama library.
	/// </summary>
	[Post("/api/pull")]
	Task<ModelOperationResponse> PullAsync(
		PullModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Downloads a model from the Ollama library. Returns the raw response so the corresponding As-stream method can read its newline-delimited progress updates.
	/// </summary>
	[Post("/api/pull")]
	Task<HttpResponseMessage> PullStreamInternalAsync(
		PullModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Uploads a model to the Ollama library.
	/// </summary>
	[Post("/api/push")]
	Task<ModelOperationResponse> PushAsync(
		PushModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Uploads a model to the Ollama library. Returns the raw response so the corresponding As-stream method can read its newline-delimited progress updates.
	/// </summary>
	[Post("/api/push")]
	Task<HttpResponseMessage> PushStreamInternalAsync(
		PushModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a model from a modelfile.
	/// </summary>
	[Post("/api/create")]
	Task<ModelOperationResponse> CreateAsync(
		CreateModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Creates a model from a modelfile. Returns the raw response so the corresponding As-stream method can read its newline-delimited progress updates.
	/// </summary>
	[Post("/api/create")]
	Task<HttpResponseMessage> CreateStreamInternalAsync(
		CreateModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of all models currently available in local Ollama storage.
	/// </summary>
	[Get("/api/tags")]
	Task<TagsResponse> GetTagsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Pulls a new model based on the specified request, streaming updates as they occur.
	/// The model's stream must be set to true
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async IAsyncEnumerable<ModelOperationResponse> PullAsStreamAsync(
		PullModelRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var response = await PullStreamInternalAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		await foreach (var update in ReadNewlineDelimitedJsonAsync<ModelOperationResponse>(response.Content.ReadAsStream(cancellationToken), cancellationToken))
		{
			yield return update;
		}
	}

	/// <summary>
	/// Pushes a new model based on the specified request, streaming updates as they occur.
	/// The model's stream must be set to true
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async IAsyncEnumerable<ModelOperationResponse> PushAsStreamAsync(
		PushModelRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var response = await PushStreamInternalAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		await foreach (var update in ReadNewlineDelimitedJsonAsync<ModelOperationResponse>(response.Content.ReadAsStream(cancellationToken), cancellationToken))
		{
			yield return update;
		}
	}

	/// <summary>
	/// Creates a new model based on the specified request, streaming updates as they occur.
	/// The model's stream must be set to true
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async IAsyncEnumerable<ModelOperationResponse> CreateAsStreamAsync(
		CreateModelRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var response = await CreateStreamInternalAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		await foreach (var update in ReadNewlineDelimitedJsonAsync<ModelOperationResponse>(response.Content.ReadAsStream(cancellationToken), cancellationToken))
		{
			yield return update;
		}
	}

	/// <summary>
	/// Deletes a model, yielding any status updates the server sends. A successful delete sends none.
	/// </summary>
	public async IAsyncEnumerable<ModelOperationResponse> DeleteAsStreamAsync(
		DeleteModelRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var response = await DeleteStreamInternalAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		await foreach (var update in ReadNewlineDelimitedJsonAsync<ModelOperationResponse>(response.Content.ReadAsStream(cancellationToken), cancellationToken))
		{
			yield return update;
		}
	}

	private static async IAsyncEnumerable<T> ReadNewlineDelimitedJsonAsync<T>(
		Stream stream,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var reader = new StreamReader(stream);
		string? line;
		while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				yield return JsonSerializer.Deserialize<T>(line)!;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				yield break;
			}
		}
	}
}
