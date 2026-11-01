using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// A request as the stub transport received it, with its body read before the client disposed it.
/// </summary>
internal sealed record RecordedRequest(
	HttpMethod Method,
	Uri Uri,
	string? MediaType,
	byte[] Body,
	AuthenticationHeaderValue? Authorization)
{
	public string BodyText => Encoding.UTF8.GetString(Body);

	public JsonNode Json => JsonNode.Parse(BodyText)!;
}

/// <summary>
/// Answers each request with the next queued response, recording what was sent.
/// </summary>
internal sealed class StubTransport : HttpMessageHandler
{
	private readonly Queue<Func<HttpResponseMessage>> _responses = new();

	public List<RecordedRequest> Requests { get; } = [];

	public RecordedRequest LastRequest => Requests[^1];

	public StubTransport Then(HttpResponseMessage response)
	{
		_responses.Enqueue(() => response);
		return this;
	}

	/// <summary>
	/// Fails the next request as an unreachable server would.
	/// </summary>
	public StubTransport ThenUnreachable()
	{
		_responses.Enqueue(() => throw new HttpRequestException("Connection refused (ollama.test:11434)"));
		return this;
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		var body = request.Content is null
			? []
			: await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

		Requests.Add(new RecordedRequest(
			request.Method,
			request.RequestUri!,
			request.Content?.Headers.ContentType?.MediaType,
			body,
			request.Headers.Authorization));

		var response = _responses.Dequeue()();
		response.RequestMessage = request;

		return response;
	}
}

/// <summary>
/// Builds the responses the stub transport returns.
/// </summary>
internal static class Respond
{
	private static readonly string _fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures");

	public static string FixtureText(string name) => File.ReadAllText(Path.Combine(_fixtures, name));

	public static byte[] FixtureBytes(string name) => File.ReadAllBytes(Path.Combine(_fixtures, name));

	public static HttpResponseMessage Json(string body) => Json(HttpStatusCode.OK, body);

	public static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
	{
		Content = new StringContent(body, Encoding.UTF8, "application/json")
	};

	public static HttpResponseMessage Fixture(string name) => Json(FixtureText(name));

	public static HttpResponseMessage Fixture(HttpStatusCode status, string name) => Json(status, FixtureText(name));

	public static HttpResponseMessage Text(string body) => new(HttpStatusCode.OK)
	{
		Content = new StringContent(body, Encoding.UTF8, "text/plain")
	};

	public static HttpResponseMessage Empty(HttpStatusCode status) => new(status);

	/// <summary>
	/// A chunked response with no declared length, as Ollama sends a stream.
	/// </summary>
	public static HttpResponseMessage Streamed(string mediaType, byte[] body)
	{
		var content = new StreamContent(new NonSeekableStream(body));
		content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

		return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
	}

	public static HttpResponseMessage Streamed(string mediaType, string body)
		=> Streamed(mediaType, Encoding.UTF8.GetBytes(body));
}

/// <summary>
/// A read-only stream that cannot report its length, so content built on it has none.
/// </summary>
internal sealed class NonSeekableStream(byte[] bytes) : Stream
{
	private readonly MemoryStream _inner = new(bytes);

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => throw new NotSupportedException();

	public override long Position
	{
		get => throw new NotSupportedException();
		set => throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

	public override void Flush() => _inner.Flush();

	public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

	public override void SetLength(long value) => throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_inner.Dispose();
		}

		base.Dispose(disposing);
	}
}

/// <summary>
/// Keeps every message logged at Debug or above.
/// </summary>
internal sealed class ListLogger : ILogger
{
	public List<string> Messages { get; } = [];

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
		=> Messages.Add(formatter(state, exception));
}

/// <summary>
/// Clients that talk to a <see cref="StubTransport"/> instead of a server.
/// </summary>
internal static class Offline
{
	public static readonly Uri ServerUri = new("http://ollama.test:11434");

	public static OllamaClient Client(StubTransport transport)
		=> Client(transport, new ListLogger());

	public static OllamaClient Client(StubTransport transport, ILogger logger)
		=> new(new OllamaClientOptions { Uri = ServerUri, Logger = logger }, transport);

	public static CancellationToken Token => TestContext.Current.CancellationToken;
}
