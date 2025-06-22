using Ollama.Api.Interfaces;
using Refit;

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
			BaseAddress = ollamaClientOptions.Uri
		};

		Generate = RestService.For<IGenerateApi>(_httpClient);
		Embeddings = RestService.For<IEmbeddingsApi>(_httpClient);
	}

	public IGenerateApi Generate { get; }

	public IEmbeddingsApi Embeddings { get; }

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
