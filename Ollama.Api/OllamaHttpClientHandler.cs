using Microsoft.Extensions.Logging;

namespace Ollama.Api;

/// <summary>
/// Logs each request and response at Debug level.
/// </summary>
/// <remarks>
/// A delegating handler, so that a stub transport can stand beneath it in a test.
/// </remarks>
internal class OllamaHttpClientHandler(OllamaClientOptions ollamaClientOptions) : DelegatingHandler
{
	private readonly ILogger _logger = ollamaClientOptions.Logger;

	// Inference and embedding calls use POST but change nothing on the server, so they are allowed by path.
	private static readonly HashSet<string> _readOnlyPostPaths = new(StringComparer.OrdinalIgnoreCase)
	{
		"/api/chat",
		"/api/generate",
		"/api/embed",
		"/api/embeddings",
		"/api/show",
		"/v1/systemone",
		"/v1/audio/speech",
		"/v1/audio/transcriptions",
		"/v1/audio/translations"
	};

	private static bool ChangesServer(HttpRequestMessage request)
	{
		if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head || request.Method == HttpMethod.Options)
		{
			return false;
		}

		var path = request.RequestUri?.AbsolutePath.TrimEnd('/') ?? string.Empty;

		return request.Method != HttpMethod.Post || !_readOnlyPostPaths.Contains(path);
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (ollamaClientOptions.IsReadOnly && ChangesServer(request))
		{
			throw new InvalidOperationException(
				$"The client is read-only; {request.Method} {request.RequestUri?.AbsolutePath} would change the server.");
		}

		var requestId = Guid.NewGuid();
		var logging = _logger.IsEnabled(LogLevel.Debug);

		if (logging)
		{
			await LogRequestAsync(requestId, request, cancellationToken);
		}

		var response = await base.SendAsync(request, cancellationToken);

		if (logging)
		{
			await LogResponseAsync(requestId, request, response, cancellationToken);
		}

		return response;
	}

	private async Task LogRequestAsync(
		Guid requestId,
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// An uploaded audio file or blob can be hundreds of megabytes of binary: name it, never read it.
		if (request.Content is null || !IsText(request.Content))
		{
			_logger.LogDebug(
				"[{RequestId}] Sending {Method} request to {Uri} with {ContentType} content",
				requestId,
				request.Method,
				request.RequestUri,
				request.Content?.Headers.ContentType?.MediaType ?? "no");

			return;
		}

		_logger.LogDebug(
			"[{RequestId}] Sending {Method} request to {Uri} with content: {Content}",
			requestId,
			request.Method,
			request.RequestUri,
			await request.Content.ReadAsStringAsync(cancellationToken));
	}

	/// <remarks>
	/// A streamed response must not be logged here. Reading the body to a string consumes the
	/// stream, so the caller that asked for it receives an empty one - and it blocks until the
	/// generation finishes, which defeats the whole point of streaming even when nothing breaks.
	/// With Debug logging on, an unguarded read here once killed a consuming process silently.
	/// </remarks>
	private async Task LogResponseAsync(
		Guid requestId,
		HttpRequestMessage request,
		HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
		if (IsStreamed(response))
		{
			_logger.LogDebug(
				"[{RequestId}] Received response with status code {StatusCode} ({StatusCodeInt}) from {Uri} with no content logged",
				requestId,
				response.StatusCode,
				(int)response.StatusCode,
				request.RequestUri);

			return;
		}

		var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

		_logger.LogDebug(
			"[{RequestId}] Received response with status code {StatusCode} ({StatusCodeInt}) from {Uri}: {Content}",
			requestId,
			response.StatusCode,
			(int)response.StatusCode,
			request.RequestUri,
			responseContent);
	}

	/// <summary>
	/// True when the response body is a stream the caller intends to read incrementally, or is not
	/// text, and which must therefore not be consumed here.
	/// </summary>
	/// <remarks>
	/// Deliberately conservative: a false positive costs one Debug log line; a false negative destroys
	/// the caller's response body.
	/// </remarks>
	internal static bool IsStreamed(HttpResponseMessage response)
		=> response.Content.Headers.ContentLength is null || !IsText(response.Content);

	/// <summary>
	/// True for a JSON or plain-text body that is read whole, never for audio, multipart, binary,
	/// newline-delimited JSON or server-sent events.
	/// </summary>
	internal static bool IsText(HttpContent content)
	{
		var mediaType = content.Headers.ContentType?.MediaType;

		if (mediaType is null
			|| mediaType.Contains("ndjson", StringComparison.OrdinalIgnoreCase)
			|| mediaType.Equals("text/event-stream", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return mediaType.EndsWith("json", StringComparison.OrdinalIgnoreCase)
			|| mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
	}
}
