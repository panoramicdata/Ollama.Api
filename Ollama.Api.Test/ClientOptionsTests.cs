using AwesomeAssertions;
using System.Net;
using System.Net.Http.Headers;

namespace Ollama.Api.Test;

/// <summary>
/// Tests for the client options that need no Ollama server: the optional API key and the request
/// timeout.
/// </summary>
/// <remarks>
/// Deliberately not derived from <see cref="Test"/>. That base constructs a client against the
/// configured server, and these are questions about how a request is shaped rather than about what
/// Ollama answers.
/// </remarks>
public class ClientOptionsTests
{
	private static readonly Uri _uri = new("http://pdl-rune-02.panoramicdata.com:11434");

	/// <summary>Records the request it was given and answers with an empty 200.</summary>
	private sealed class RecordingHandler : HttpMessageHandler
	{
		public HttpRequestMessage? LastRequest { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			LastRequest = request;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("{}")
			});
		}
	}

	private static async Task<HttpRequestMessage> SendThroughAuthenticationAsync(string? apiKey)
	{
		var recorder = new RecordingHandler();

		using var handler = new OllamaAuthenticationHandler(apiKey) { InnerHandler = recorder };
		using var invoker = new HttpMessageInvoker(handler);
		using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_uri, "/api/chat"));

		using var response = await invoker
			.SendAsync(request, TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		return recorder.LastRequest!;
	}

	/// <summary>
	/// A configured key is sent as a bearer token.
	/// </summary>
	[Fact]
	public async Task ApiKeySet_IsSentAsABearerToken()
	{
		var request = await SendThroughAuthenticationAsync("sk-abc123").ConfigureAwait(true);

		request.Headers.Authorization.Should().BeEquivalentTo(
			new AuthenticationHeaderValue("Bearer", "sk-abc123"));
	}

	/// <summary>
	/// No key — missing, empty or blank — means no Authorization header at all.
	/// </summary>
	/// <param name="apiKey">The configured key, in each of its absent forms.</param>
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task NoApiKey_SendsNoAuthorizationHeader(string? apiKey)
	{
		var request = await SendThroughAuthenticationAsync(apiKey).ConfigureAwait(true);

		request.Headers.Authorization.Should().BeNull(
			"a local Ollama needs no authentication, and an empty Bearer would be rejected by one that does");
	}

	/// <summary>
	/// With no timeout configured, the long-standing default applies.
	/// </summary>
	[Fact]
	public void NoTimeoutGiven_KeepsTheLongDefault()
		=> new OllamaClientOptions { Uri = _uri }
			.EffectiveTimeout.Should().Be(
				TimeSpan.FromMinutes(30),
				"model retrieval can take a very long time, and that default predates this option");

	/// <summary>
	/// A configured timeout is the one applied.
	/// </summary>
	[Fact]
	public void TimeoutGiven_IsUsed()
		=> new OllamaClientOptions { Uri = _uri, Timeout = TimeSpan.FromMinutes(5) }
			.EffectiveTimeout.Should().Be(TimeSpan.FromMinutes(5));

	/// <summary>
	/// A zero or negative timeout is refused rather than applied.
	/// </summary>
	/// <param name="seconds">The offending timeout, in seconds.</param>
	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void TimeoutThatIsNotPositive_IsRejected(int seconds)
	{
		var act = () => new OllamaClientOptions
		{
			Uri = _uri,
			Timeout = TimeSpan.FromSeconds(seconds)
		}.EffectiveTimeout;

		act.Should().Throw<InvalidOperationException>(
			"a zero or negative timeout would fail every call immediately, which reads as the server being down");
	}
}
