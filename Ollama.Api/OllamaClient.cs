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
