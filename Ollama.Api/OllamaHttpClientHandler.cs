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

		if (_logger.IsEnabled(LogLevel.Debug))
		{
			if (request.Content is not null)
			{
				_logger.LogDebug(
					"[{RequestId}] Sending {Method} request to {Uri} with content: {Content}",
					requestId,
					request.Method,
					request.RequestUri,
					await request.Content.ReadAsStringAsync(cancellationToken));
			}
			else
			{
				_logger.LogDebug(
					"[{RequestId}] Sending {Method} request to {Uri} with no content",
					requestId,
					request.Method,
					request.RequestUri);
			}
		}

		var response = await base.SendAsync(request, cancellationToken);

		if(_logger.IsEnabled(LogLevel.Debug))
		{
			if(response.Content is not null)
			{
				var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogDebug(
				"[{RequestId}] Received response with status code {StatusCode} ({StatusCodeInt}) from {Uri}: {Content}",
				requestId,
				response.StatusCode,
				(int)response.StatusCode,
				request.RequestUri,
				responseContent);
			}
			else
			{
				_logger.LogDebug(
					"[{RequestId}] Received response with status code {StatusCode} ({StatusCodeInt}) from {Uri} with no content",
					requestId,
					response.StatusCode,
					(int)response.StatusCode,
					request.RequestUri);
			}
		}

		return response;
	}
}