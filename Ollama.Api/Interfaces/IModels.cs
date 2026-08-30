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
	[Post("/api/delete")]
	Task<ModelOperationResponse> DeleteAsync(
		DeleteModelRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Deletes a model. Returns the raw response so the corresponding As-stream method can read its newline-delimited progress updates.
	/// </summary>
	[Post("/api/delete")]
	Task<HttpResponseMessage> DeleteStreamInternalAsync(
		DeleteModelRequest request,
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
	/// Creates a new model based on the specified request, streaming updates as they occur.
	/// The model's stream must be set to true
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
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
