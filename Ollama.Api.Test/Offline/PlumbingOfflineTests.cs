using AwesomeAssertions;
using Ollama.Api.Models;
using System.Net;
using System.Net.Http.Headers;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// The client's plumbing: logging, error extraction, request bodies and construction.
/// </summary>
public class PlumbingOfflineTests
{
	/// <summary>
	/// A JSON request and a buffered JSON response are both logged in full.
	/// </summary>
	[Fact]
	public async Task Logging_JsonRequestAndResponse_AreLoggedInFull()
	{
		var logger = new ListLogger();
		var transport = new StubTransport().Then(Respond.Json("""{"version":"0.40.13"}"""));
		using var client = Offline.Client(transport, logger);

		await client.Embeddings.EmbedAsync(new EmbedRequest { Model = "m", Input = ["logged-input"] }, Offline.Token);

		logger.Messages.Should().HaveCount(2);
		logger.Messages[0].Should().Contain("logged-input");
		logger.Messages[1].Should().Contain("0.40.13");
	}

	/// <summary>
	/// Streamed audio is neither read nor logged, so the caller receives every byte.
	/// </summary>
	[Fact]
	public async Task Logging_StreamedAudio_IsLeftForTheCaller()
	{
		var logger = new ListLogger();
		var audioBytes = Respond.FixtureBytes("speech-start.wav");
		var transport = new StubTransport().Then(Respond.Streamed("audio/wav", audioBytes));
		using var client = Offline.Client(transport, logger);

		await using var audio = await client.Audio.CreateSpeechAsync(new SpeechRequest { Model = "m", Input = "i" }, Offline.Token);
		using var copy = new MemoryStream();
		await audio.Content.CopyToAsync(copy, Offline.Token);

		copy.ToArray().Should().Equal(audioBytes);
		logger.Messages[1].Should().Contain("no content logged");
	}

	/// <summary>
	/// An uploaded file is named in the log, never read into it.
	/// </summary>
	[Fact]
	public async Task Logging_MultipartUpload_IsNotRead()
	{
		var logger = new ListLogger();
		var transport = new StubTransport().Then(Respond.Fixture("transcription.json"));
		using var client = Offline.Client(transport, logger);
		using var wav = new MemoryStream(Respond.FixtureBytes("speech-start.wav"));

		await client.Audio.TranscribeAsync(new TranscriptionRequest { Model = "m", Audio = wav }, Offline.Token);

		logger.Messages[0].Should().Contain("multipart/form-data content").And.NotContain("RIFF");
	}

	/// <summary>
	/// A request with no body is logged as such.
	/// </summary>
	[Fact]
	public async Task Logging_RequestWithoutBody_SaysSo()
	{
		var logger = new ListLogger();
		var transport = new StubTransport().Then(Respond.Json("""{"models":[]}"""));
		using var client = Offline.Client(transport, logger);

		await client.Utility.GetPsAsync(Offline.Token);

		logger.Messages[0].Should().Contain("with no content");
	}

	/// <summary>
	/// Only whole JSON and plain-text bodies count as text that may be read for logging.
	/// </summary>
	/// <param name="mediaType">The body's media type, or null for none.</param>
	/// <param name="expected">Whether it counts as text.</param>
	[Theory]
	[InlineData("application/json", true)]
	[InlineData("text/plain", true)]
	[InlineData("application/x-ndjson", false)]
	[InlineData("text/event-stream", false)]
	[InlineData("audio/wav", false)]
	[InlineData("multipart/form-data", false)]
	[InlineData(null, false)]
	public void IsText_RecognisesOnlyWholeTextBodies(string? mediaType, bool expected)
	{
		using var content = new ByteArrayContent([]);
		content.Headers.ContentType = mediaType is null ? null : new MediaTypeHeaderValue(mediaType);

		OllamaHttpClientHandler.IsText(content).Should().Be(expected);
	}

	/// <summary>
	/// The message is read from either error shape, and anything else falls back sensibly.
	/// </summary>
	/// <param name="body">The error body.</param>
	/// <param name="expected">What should be extracted.</param>
	[Theory]
	[InlineData(null, null)]
	[InlineData("  ", null)]
	[InlineData("""{"error":"native"}""", "native")]
	[InlineData("""{"error":{"message":"openai","type":"invalid_request_error"}}""", "openai")]
	[InlineData("""{"error":{"type":"no message"}}""", """{"error":{"type":"no message"}}""")]
	[InlineData("""{"error":42}""", """{"error":42}""")]
	[InlineData("""{"status":"other"}""", """{"status":"other"}""")]
	[InlineData("""["array"]""", """["array"]""")]
	[InlineData("not json", "not json")]
	public void OllamaError_ExtractsTheMessage(string? body, string? expected)
		=> OllamaError.Extract(body).Should().Be(expected);

	/// <summary>
	/// A caller's stream sent as a body reports its remaining length and is left open.
	/// </summary>
	[Fact]
	public async Task LeaveOpenStreamContent_ReportsLengthAndLeavesTheStreamOpen()
	{
		using var source = new MemoryStream([1, 2, 3, 4]);
		source.Position = 1;

		using (var content = new LeaveOpenStreamContent(source))
		{
			content.Headers.ContentLength.Should().Be(3);
			(await content.ReadAsByteArrayAsync(Offline.Token)).Should().Equal(2, 3, 4);
		}

		source.CanRead.Should().BeTrue();
	}

	/// <summary>
	/// A stream that cannot seek has no length to report, and is still sent whole.
	/// </summary>
	[Fact]
	public async Task LeaveOpenStreamContent_NonSeekable_HasNoLength()
	{
		using var source = new NonSeekableStream([5, 6]);
		using var content = new LeaveOpenStreamContent(source);

		content.Headers.ContentLength.Should().BeNull();
		using var copy = new MemoryStream();
		await content.CopyToAsync(copy, Offline.Token);
		copy.ToArray().Should().Equal(5, 6);

		var noStream = () => new LeaveOpenStreamContent(null!);
		noStream.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// The configured API key reaches the server on the new endpoints too.
	/// </summary>
	[Fact]
	public async Task ApiKey_IsSentToTheNewEndpoints()
	{
		var transport = new StubTransport().Then(Respond.Fixture("systemone-choice.json"));
		using var client = new OllamaClient(new OllamaClientOptions { Uri = Offline.ServerUri, ApiKey = "key" }, transport);

		await client.Decisions.DecideAsync(new DecisionRequest
		{
			Model = "clef:cloud",
			State = "s",
			Questions = new() { ["q"] = new NoulQuestion { Instructions = "?" } }
		}, Offline.Token);

		transport.LastRequest.Authorization!.Parameter.Should().Be("key");
	}

	/// <summary>
	/// A client cannot be built without options or a transport.
	/// </summary>
	[Fact]
	public void Construction_RequiresOptionsAndTransport()
	{
		using var transport = new StubTransport();
		var noOptions = () => new OllamaClient(null!, transport);
		var noTransport = () => new OllamaClient(new OllamaClientOptions { Uri = Offline.ServerUri }, null!);

		noOptions.Should().Throw<ArgumentNullException>();
		noTransport.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// The public constructor builds a client over the real transport, which disposes cleanly twice.
	/// </summary>
	[Fact]
	public void PublicConstructor_BuildsAndDisposes()
	{
		var client = new OllamaClient(new OllamaClientOptions { Uri = Offline.ServerUri });

		client.Audio.Should().NotBeNull();
		client.Decisions.Should().NotBeNull();
		client.Dispose();
		client.Dispose();
	}

	/// <summary>
	/// Response statuses Ollama uses for a blob that already exists are both treated as success.
	/// </summary>
	/// <param name="status">The status the server answers with.</param>
	[Theory]
	[InlineData(HttpStatusCode.OK)]
	[InlineData(HttpStatusCode.Created)]
	public async Task PushBlob_AcceptsBothSuccessStatuses(HttpStatusCode status)
	{
		var transport = new StubTransport().Then(Respond.Empty(status));
		using var client = Offline.Client(transport);
		using var blob = new MemoryStream([1]);

		await client.Models.Invoking(m => m.PushBlobAsync("sha256:00", blob, Offline.Token)).Should().NotThrowAsync();
	}
}
