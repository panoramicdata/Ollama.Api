using Ollama.Api.Interfaces;
using Ollama.Api.Models;
using Refit;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ollama.Api;

public class OllamaClient : IDisposable
{
	private bool _disposedValue;
	private readonly OllamaHttpClientHandler _httpClientHandler;
	private readonly HttpClient _httpClient;

	public OllamaClient(OllamaClientOptions ollamaClientOptions)
	{
		ArgumentNullException.ThrowIfNull(ollamaClientOptions);
		_httpClientHandler = new OllamaHttpClientHandler(ollamaClientOptions);
		_httpClient = new HttpClient(_httpClientHandler)
		{
			BaseAddress = ollamaClientOptions.Uri,
			Timeout = TimeSpan.FromMinutes(30) // Some API calls such as model retrieval can take a very long time.
		};

		Generate = RestService.For<IGenerate>(_httpClient);
		Embeddings = RestService.For<IEmbeddings>(_httpClient);
		Chat = RestService.For<IChat>(_httpClient);
		Models = RestService.For<IModels>(_httpClient);
		Utility = RestService.For<IUtility>(_httpClient);
	}

	/// <inheritdoc />
	public IGenerate Generate { get; }

	/// <inheritdoc />
	public IEmbeddings Embeddings { get; }

	/// <inheritdoc />
	public IChat Chat { get; }

	/// <inheritdoc />
	public IModels Models { get; }

	/// <inheritdoc />
	public IUtility Utility { get; }

	public async IAsyncEnumerable<ModelOperationResponse> CreateModelStreamAsync(
		CreateModelRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await Models.CreateStreamAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		await foreach (var update in ReadNewlineDelimitedJsonAsync(response.Content.ReadAsStream(cancellationToken), cancellationToken))
		{
			yield return update;
		}
	}

	public static async IAsyncEnumerable<ModelOperationResponse> ReadNewlineDelimitedJsonAsync(
	Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using var reader = new StreamReader(stream);
		while (!reader.EndOfStream)
		{
			var line = await reader.ReadLineAsync(cancellationToken);
			if (!string.IsNullOrWhiteSpace(line))
			{
				yield return JsonSerializer.Deserialize<ModelOperationResponse>(line)!;
			}

			if (cancellationToken.IsCancellationRequested)
				yield break;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_httpClient.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
