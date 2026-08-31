using System.Net.Http.Headers;

namespace Ollama.Api;

/// <summary>
/// Adds the configured API key to every request as a bearer token.
/// </summary>
/// <param name="apiKey">
/// The key, or null when the server needs none — which is the normal case for a local Ollama.
/// </param>
/// <remarks>
/// A delegating handler rather than a line inside <see cref="OllamaHttpClientHandler"/>, because that
/// one is terminal: it derives from <see cref="HttpClientHandler"/> and actually sends the request, so
/// nothing can stand in for it in a test. This can be exercised with a stub inner handler, which is
/// the difference between "the header is added" being asserted and being assumed.
/// </remarks>
internal sealed class OllamaAuthenticationHandler(string? apiKey) : DelegatingHandler
{
	private readonly string? _apiKey = apiKey;

	/// <inheritdoc />
	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// Nothing is sent when there is no key. An empty bearer token is not the same as no
		// authentication: a server that requires one rejects it, and a server that does not may still
		// object to a malformed header.
		if (!string.IsNullOrWhiteSpace(_apiKey))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
		}

		return base.SendAsync(request, cancellationToken);
	}
}
