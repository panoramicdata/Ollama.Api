using Ollama.Api.Interfaces;
using Refit;
using System.Text.Json;

namespace Ollama.Api;

public class OllamaClient : IDisposable
{
	private bool _disposedValue;
	private readonly HttpClient _httpClient;

	public OllamaClient(OllamaClientOptions ollamaClientOptions)
	{
		ArgumentNullException.ThrowIfNull(ollamaClientOptions);
		var httpClientHandler = new OllamaHttpClientHandler(ollamaClientOptions);
		_httpClient = new HttpClient(httpClientHandler)
		{
			BaseAddress = ollamaClientOptions.Uri,
			Timeout = TimeSpan.FromMinutes(30) // Some API calls such as model retrieval can take a very long time.
		};

		var serializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Or whatever policy you use
															   // Add other global options here
		};

		var refitSettings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions)
		};

		Generate = RestService.For<IGenerate>(_httpClient, refitSettings);
		Embeddings = RestService.For<IEmbeddings>(_httpClient, refitSettings);
		Chat = RestService.For<IChat>(_httpClient, refitSettings);
		Models = RestService.For<IModels>(_httpClient, refitSettings);
		Utility = RestService.For<IUtility>(_httpClient, refitSettings);
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
