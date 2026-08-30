using Microsoft.Extensions.Logging;

namespace Ollama.Api;

internal class OllamaHttpClientHandler(OllamaClientOptions ollamaClientOptions) : HttpClientHandler
{
	private readonly ILogger _logger = ollamaClientOptions.Logger;

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
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
		if (request.Content is null)
		{
			_logger.LogDebug(
				"[{RequestId}] Sending {Method} request to {Uri} with no content",
				requestId,
				request.Method,
				request.RequestUri);

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
	/// Ollama sends NDJSON with no Content-Length for a streamed call and application/json with a
	/// length for a buffered one, so the two are distinguishable without the handler needing to
	/// know what was requested.
	///
	/// This cost a day of debugging: with Debug logging on - which is the default in a development
	/// environment - ChatStreamAsync appeared to work standalone and killed the consuming process,
	/// silently, as soon as it ran anywhere that had the logger enabled.
	/// </remarks>
	private async Task LogResponseAsync(
		Guid requestId,
		HttpRequestMessage request,
		HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
		if (response.Content is null || IsStreamed(response))
		{
			_logger.LogDebug(
				"[{RequestId}] Received response with status code {StatusCode} ({StatusCodeInt}) from {Uri} with no content",
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
	/// True when the response body is a stream the caller intends to read incrementally, and which
	/// must therefore not be consumed here.
	/// </summary>
	/// <remarks>
	/// Deliberately conservative: anything without a declared length, or declaring a newline
	/// delimited content type, is treated as streamed. A false positive costs one Debug log line; a
	/// false negative destroys the caller's response body.
	/// </remarks>
	private static bool IsStreamed(HttpResponseMessage response)
		=> response.Content.Headers.ContentLength is null
		|| (response.Content.Headers.ContentType?.MediaType?.Contains("ndjson", StringComparison.OrdinalIgnoreCase) ?? false);
}