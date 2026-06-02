using Ollama.Api.Interfaces;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ollama.Api;

public class OllamaClient : IDisposable
{
	private bool _disposedValue;
	private readonly HttpClient _httpClient;

	internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters =
		{
			// Convert items in models as "object" or "object?" to best c# type e.g. string, long, bool, double, etc. instead of JsonElement which is the default for "object".
			new ObjectToInferredTypesConverter(),
			new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
		}
	};

	public OllamaClient(OllamaClientOptions ollamaClientOptions)
	{
		ArgumentNullException.ThrowIfNull(ollamaClientOptions);
		var httpClientHandler = new OllamaHttpClientHandler(ollamaClientOptions);
		_httpClient = new HttpClient(httpClientHandler)
		{
			BaseAddress = ollamaClientOptions.Uri,
			Timeout = TimeSpan.FromMinutes(30) // Some API calls such as model retrieval can take a very long time.
		};

		var refitSettings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(JsonSerializerOptions)
		};

		Generate = RestService.For<IGenerate>(_httpClient, refitSettings);
		Embeddings = RestService.For<IEmbeddings>(_httpClient, refitSettings);
		var chatApi = RestService.For<IChatApi>(_httpClient, refitSettings);
		Chat = new ChatClient(chatApi);
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
