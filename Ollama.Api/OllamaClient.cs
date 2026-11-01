using Ollama.Api.Interfaces;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ollama.Api;

/// <summary>
/// A client for a single Ollama server. Each area of the API is reached through one of its
/// properties.
/// </summary>
public class OllamaClient : IDisposable
{
	private bool _disposedValue;
	private readonly HttpClient _httpClient;

	internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		// Decision answers are polymorphic on "type", which Ollama does not promise to send first.
		AllowOutOfOrderMetadataProperties = true,
		Converters =
		{
			// Convert items in models as "object" or "object?" to best c# type e.g. string, long, bool, double, etc. instead of JsonElement which is the default for "object".
			new ObjectToInferredTypesConverter(),
			new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
		}
	};

	/// <summary>
	/// Creates a client for the server described by the supplied options.
	/// </summary>
	/// <param name="ollamaClientOptions">Where the server is, and where to log.</param>
	/// <exception cref="ArgumentNullException"><paramref name="ollamaClientOptions"/> is null.</exception>
	public OllamaClient(OllamaClientOptions ollamaClientOptions)
		: this(ollamaClientOptions, new HttpClientHandler())
	{
	}

	/// <summary>
	/// Creates a client that sends its requests through <paramref name="transport"/>, so that a test
	/// can answer them without a server.
	/// </summary>
	internal OllamaClient(OllamaClientOptions ollamaClientOptions, HttpMessageHandler transport)
	{
		ArgumentNullException.ThrowIfNull(ollamaClientOptions);
		ArgumentNullException.ThrowIfNull(transport);

		// Authentication above logging, so the logged request is the one actually sent.
		var handler = new OllamaAuthenticationHandler(ollamaClientOptions.ApiKey)
		{
			InnerHandler = new OllamaHttpClientHandler(ollamaClientOptions) { InnerHandler = transport }
		};

		_httpClient = new HttpClient(handler)
		{
			BaseAddress = ollamaClientOptions.Uri,
			Timeout = ollamaClientOptions.EffectiveTimeout
		};

		var refitSettings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(JsonSerializerOptions)
		};

		Generate = RestService.For<IGenerate>(_httpClient, refitSettings);
		Embeddings = RestService.For<IEmbeddings>(_httpClient, refitSettings);
		var chatApi = RestService.For<IChatApi>(_httpClient, refitSettings);
		Chat = new ChatClient(chatApi, _httpClient);
		Models = RestService.For<IModels>(_httpClient, refitSettings);
		Utility = RestService.For<IUtility>(_httpClient, refitSettings);
		Decisions = RestService.For<IDecisions>(_httpClient, refitSettings);
		Audio = new AudioClient(_httpClient, refitSettings);
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

	/// <inheritdoc />
	public IDecisions Decisions { get; }

	/// <inheritdoc />
	public IAudio Audio { get; }

	/// <summary>
	/// Releases the underlying <see cref="HttpClient"/>.
	/// </summary>
	/// <param name="disposing">
	/// True when called from <see cref="Dispose()"/>, false when called from a finalizer.
	/// </param>
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

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
