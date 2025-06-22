using Microsoft.Extensions.Logging;

namespace Ollama.Api;

internal class OllamaHttpClientHandler(OllamaClientOptions ollamaClientOptions) : HttpClientHandler
{
	private readonly ILogger _logger = ollamaClientOptions.Logger;

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_logger.LogDebug("Sending request to {Uri}", request.RequestUri);
		var response = await base.SendAsync(request, cancellationToken);
		_logger.LogDebug("Received response with status code {StatusCode} from {Uri}", response.StatusCode, request.RequestUri);

		// Log response content if needed
		if (response.IsSuccessStatusCode)
		{
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogDebug("Response content: {Content}", responseContent);
		}
		else
		{
			_logger.LogError("Error response from {Uri}: {StatusCode} - {ReasonPhrase}", request.RequestUri, response.StatusCode, response.ReasonPhrase);
		}

		return response;
	}
}