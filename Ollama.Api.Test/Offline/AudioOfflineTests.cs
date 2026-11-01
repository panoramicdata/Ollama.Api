using AwesomeAssertions;
using Ollama.Api.Models;
using Refit;
using System.Net;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// Speech, transcription and translation, answered from responses lfm3-audio gave Ollama 0.40.13.
/// </summary>
public class AudioOfflineTests
{
	private static readonly byte[] _wavStart = Respond.FixtureBytes("speech-start.wav");

	/// <summary>
	/// Speech is returned as the WAV stream the server sent, with every request field on the wire.
	/// </summary>
	[Fact]
	public async Task CreateSpeech_ReturnsTheAudioStream()
	{
		var transport = new StubTransport().Then(Respond.Streamed("audio/wav", _wavStart));
		using var client = Offline.Client(transport);

		await using var audio = await client.Audio.CreateSpeechAsync(new SpeechRequest
		{
			Model = "lfm3-audio",
			Input = "Hello from Panoramic Data.",
			Voice = "female-british-english",
			ResponseFormat = SpeechFormat.Pcm,
			Speed = 1
		}, Offline.Token);

		using var copy = new MemoryStream();
		await audio.CopyToAsync(copy, Offline.Token);

		audio.ContentType.Should().Be("audio/wav");
		copy.ToArray().Should().Equal(_wavStart);
		copy.ToArray()[..4].Should().Equal("RIFF"u8.ToArray());

		var sent = transport.LastRequest;
		sent.Uri.AbsolutePath.Should().Be("/v1/audio/speech");
		sent.Json["voice"]!.GetValue<string>().Should().Be("female-british-english");
		sent.Json["response_format"]!.GetValue<string>().Should().Be("pcm");
		sent.Json["speed"]!.GetValue<double>().Should().Be(1);
	}

	/// <summary>
	/// Disposing the audio synchronously releases it too.
	/// </summary>
	[Fact]
	public async Task CreateSpeech_SynchronousDispose_ReleasesTheStream()
	{
		var transport = new StubTransport().Then(Respond.Streamed("audio/wav", _wavStart));
		using var client = Offline.Client(transport);

		var audio = await client.Audio.CreateSpeechAsync(new SpeechRequest { Model = "lfm3-audio", Input = "Hi" }, Offline.Token);
		audio.Dispose();

		var read = () => audio.Content.ReadByte();
		read.Should().Throw<ObjectDisposedException>();
	}

	/// <summary>
	/// Each refusal Ollama gives a speech request surfaces as an exception carrying its message.
	/// </summary>
	/// <param name="fixture">The captured error body.</param>
	/// <param name="message">The start of the message it carries.</param>
	[Theory]
	[InlineData("speech-unknown-voice.json", "voice \"Nobody\" is not supported")]
	[InlineData("speech-unsupported-format.json", "response_format \"mp3\" is not supported")]
	[InlineData("speech-not-speech-model.json", "\"qwen3.5:2b\" does not support speech")]
	public async Task CreateSpeech_Refused_Throws(string fixture, string message)
	{
		var transport = new StubTransport().Then(Respond.Fixture(HttpStatusCode.BadRequest, fixture));
		using var client = Offline.Client(transport);

		var refusal = await Offline.ShouldBeRefusedAsync(
			() => client.Audio.CreateSpeechAsync(new SpeechRequest { Model = "lfm3-audio", Input = "Hi" }, Offline.Token),
			HttpStatusCode.BadRequest);

		refusal.Should().StartWith(message);
	}

	/// <summary>
	/// A transcription uploads the audio and every set field as a form, and reads the text back.
	/// </summary>
	[Fact]
	public async Task Transcribe_SendsAFormAndReadsTheText()
	{
		var transport = new StubTransport().Then(Respond.Fixture("transcription.json"));
		using var client = Offline.Client(transport);
		using var wav = new MemoryStream(_wavStart);

		var response = await client.Audio.TranscribeAsync(new TranscriptionRequest
		{
			Model = "lfm3-audio",
			Audio = wav,
			FileName = "hello.wav",
			Language = "en",
			Prompt = "A greeting.",
			Temperature = 0.5,
			ResponseFormat = TranscriptionFormat.Json
		}, Offline.Token);

		response.Text.Should().Be("Hello from Panoramic Data.");

		var sent = transport.LastRequest;
		sent.Uri.AbsolutePath.Should().Be("/v1/audio/transcriptions");
		sent.MediaType.Should().Be("multipart/form-data");
		sent.BodyText.Should()
			.Contain("filename=hello.wav")
			.And.Contain("name=model").And.Contain("lfm3-audio")
			.And.Contain("name=language").And.Contain("name=prompt")
			.And.Contain("name=temperature").And.Contain("0.5")
			.And.Contain("name=response_format");
		wav.CanRead.Should().BeTrue("the caller owns the audio stream");
	}

	/// <summary>
	/// The verbose format adds the task, language, duration and timed segments.
	/// </summary>
	[Fact]
	public async Task Transcribe_Verbose_ReadsSegments()
	{
		var transport = new StubTransport().Then(Respond.Fixture("transcription-verbose.json"));
		using var client = Offline.Client(transport);
		using var wav = new MemoryStream(_wavStart);

		var response = await client.Audio.TranscribeAsync(
			new TranscriptionRequest { Model = "lfm3-audio", Audio = wav, ResponseFormat = TranscriptionFormat.VerboseJson },
			Offline.Token);

		response.Task.Should().Be("transcribe");
		response.Language.Should().Be("english");
		response.Duration.Should().Be(2.08);
		var segment = response.Segments.Should().ContainSingle().Which;
		segment.Id.Should().Be(0);
		segment.Start.Should().Be(0);
		segment.End.Should().Be(2.08);
		segment.Text.Should().Be("Hello from Panoramic Data.");
	}

	/// <summary>
	/// The text format arrives as plain text and is returned as the transcript.
	/// </summary>
	[Fact]
	public async Task Transcribe_TextFormat_ReturnsThePlainText()
	{
		var transport = new StubTransport().Then(Respond.Text("Hello from Panoramic Data.\n"));
		using var client = Offline.Client(transport);
		using var wav = new MemoryStream(_wavStart);

		var response = await client.Audio.TranscribeAsync(
			new TranscriptionRequest { Model = "lfm3-audio", Audio = wav, ResponseFormat = TranscriptionFormat.Text },
			Offline.Token);

		response.Text.Should().Be("Hello from Panoramic Data.");
	}

	/// <summary>
	/// A translation names its output language and reads the translated text.
	/// </summary>
	[Fact]
	public async Task Translate_SendsTheTargetLanguage()
	{
		var transport = new StubTransport().Then(Respond.Fixture("translation.json"));
		using var client = Offline.Client(transport);
		using var wav = new MemoryStream(_wavStart);

		var response = await client.Audio.TranslateAsync(
			new TranslationRequest { Model = "lfm3-audio", Audio = wav, ToLanguage = "fr" },
			Offline.Token);

		response.Text.Should().Be("Bonjour de Panoramic Data.");
		transport.LastRequest.Uri.AbsolutePath.Should().Be("/v1/audio/translations");
		transport.LastRequest.BodyText.Should().Contain("name=to_language").And.Contain("fr");
	}

	/// <summary>
	/// A streamed transcription yields each event and stops at the done event.
	/// </summary>
	[Fact]
	public async Task TranscribeStream_YieldsEventsUntilDone()
	{
		var stream = ": comment\n\n" + Respond.FixtureText("transcription-stream.txt") + "\n\ndata: {\"type\":\"after-done\"}\n";
		var transport = new StubTransport().Then(Respond.Streamed("text/event-stream", stream));
		using var client = Offline.Client(transport);
		using var wav = new MemoryStream(_wavStart);

		var events = await client.Audio
			.TranscribeStreamAsync(new TranscriptionRequest { Model = "lfm3-audio", Audio = wav }, Offline.Token)
			.ToListAsync(Offline.Token);

		events.Should().HaveCount(2);
		events[0].Type.Should().Be("transcript.text.delta");
		events[0].Delta.Should().Be("Hello from panoramic data.");
		events[0].IsDone.Should().BeFalse();
		events[1].IsDone.Should().BeTrue();
		events[1].Text.Should().Be("Hello from panoramic data.");
		transport.LastRequest.BodyText.Should().Contain("name=stream");
	}

	/// <summary>
	/// A failed transcription surfaces as an exception, before any event is yielded.
	/// </summary>
	[Fact]
	public async Task TranscribeStream_Refused_Throws()
	{
		var transport = new StubTransport().Then(Respond.Json(HttpStatusCode.BadRequest, """{"error":{"message":"no audio"}}"""));
		using var client = Offline.Client(transport);
		using var wav = new MemoryStream(_wavStart);

		var refusal = await Offline.ShouldBeRefusedAsync(
			async () => await client.Audio
				.TranscribeStreamAsync(new TranscriptionRequest { Model = "lfm3-audio", Audio = wav }, Offline.Token)
				.ToListAsync(Offline.Token),
			HttpStatusCode.BadRequest);

		refusal.Should().Be("no audio");
	}

	/// <summary>
	/// Lines that carry no event are skipped, including the OpenAI end-of-stream marker.
	/// </summary>
	/// <param name="line">A line from an event stream.</param>
	[Theory]
	[InlineData("")]
	[InlineData(": keep-alive")]
	[InlineData("event: message")]
	[InlineData("data:")]
	[InlineData("data: [DONE]")]
	public void ParseEvent_LineWithoutAnEvent_IsSkipped(string line)
		=> AudioClient.ParseEvent(line).Should().BeNull();

	/// <summary>
	/// Each response format has its wire name, and none sends none.
	/// </summary>
	[Fact]
	public void ResponseFormats_HaveTheirWireNames()
	{
		AudioClient.ToWireName(TranscriptionFormat.Json).Should().Be("json");
		AudioClient.ToWireName(TranscriptionFormat.VerboseJson).Should().Be("verbose_json");
		AudioClient.ToWireName(TranscriptionFormat.Text).Should().Be("text");
		AudioClient.ToWireName(null).Should().BeNull();
		AudioClient.ToWireName((TranscriptionFormat)99).Should().BeNull();
	}

	/// <summary>
	/// Audio that arrives without a declared type reports none rather than guessing.
	/// </summary>
	[Fact]
	public async Task CreateSpeech_NoContentType_ReportsNone()
	{
		var transport = new StubTransport().Then(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(_wavStart) });
		using var client = Offline.Client(transport);

		await using var audio = await client.Audio.CreateSpeechAsync(new SpeechRequest { Model = "lfm3-audio", Input = "Hi" }, Offline.Token);

		audio.ContentType.Should().BeNull();
	}

	/// <summary>
	/// A request with nothing optional set sends only the file and the model.
	/// </summary>
	[Fact]
	public async Task BuildForm_MinimalRequest_SendsFileAndModelOnly()
	{
		using var wav = new MemoryStream(_wavStart);
		using var form = AudioClient.BuildForm(new TranscriptionRequest { Model = "lfm3-audio", Audio = wav });

		var body = await form.ReadAsStringAsync(Offline.Token);

		form.Should().HaveCount(2);
		body.Should().Contain("filename=audio.wav").And.NotContain("name=language");
	}

	/// <summary>
	/// A missing request is refused before anything is sent.
	/// </summary>
	[Fact]
	public async Task NullRequests_AreRefused()
	{
		using var client = Offline.Client(new StubTransport());

		await client.Invoking(c => c.Audio.CreateSpeechAsync(null!, Offline.Token)).Should().ThrowAsync<ArgumentNullException>();
		await client.Invoking(c => c.Audio.TranscribeAsync(null!, Offline.Token)).Should().ThrowAsync<ArgumentNullException>();
		await client.Invoking(c => c.Audio.TranslateAsync(null!, Offline.Token)).Should().ThrowAsync<ArgumentNullException>();
	}
}
