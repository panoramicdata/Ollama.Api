using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

/// <summary>
/// The Ollama 0.40 features - speech, transcription, decisions, embed, blobs and the corrected delete
/// - against a live Ollama server of 0.40.13 or later.
/// </summary>
/// <param name="fixture">The shared fixture that supplies configuration and services.</param>
/// <param name="testOutputHelper">Where log output for the test is written.</param>
public class Ollama04013Tests(Fixture fixture, ITestOutputHelper testOutputHelper) : Test(fixture, testOutputHelper)
{
	private static string SpeechModel => TestModels.GetModelName(ModelType.Lfm3Audio);

	private static string DecisionModel => TestModels.GetModelName(ModelType.D3);

	/// <summary>
	/// Text spoken by a speech model and transcribed by the same model comes back as the same words.
	/// </summary>
	[Fact]
	public async Task Speech_RoundTripsThroughTranscription()
	{
		using var wav = new MemoryStream();
		await using (var audio = await OllamaClient.Audio.CreateSpeechAsync(new SpeechRequest
		{
			Model = SpeechModel,
			Input = "Hello from Panoramic Data.",
			Voice = "female-british-english"
		}, CancellationToken))
		{
			audio.ContentType.Should().Be("audio/wav");
			await audio.CopyToAsync(wav, CancellationToken);
		}

		wav.Length.Should().BeGreaterThan(44, "a WAV holds samples after its header");
		wav.Position = 0;

		var transcript = await OllamaClient.Audio.TranscribeAsync(new TranscriptionRequest
		{
			Model = SpeechModel,
			Audio = wav,
			Language = "en",
			ResponseFormat = TranscriptionFormat.VerboseJson
		}, CancellationToken);

		transcript.Text.Should().ContainEquivalentOf("panoramic");
		transcript.Duration.Should().BePositive();
	}

	/// <summary>
	/// An unknown voice is refused with the list of voices the model has.
	/// </summary>
	[Fact]
	public async Task Speech_UnknownVoice_IsRefused()
	{
		var act = () => OllamaClient.Audio.CreateSpeechAsync(
			new SpeechRequest { Model = SpeechModel, Input = "Hi", Voice = "Nobody" },
			CancellationToken);

		var thrown = await act.Should().ThrowAsync<Refit.ApiException>();
		thrown.Which.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
		thrown.Which.Content.Should().Contain("Nobody");
	}

	/// <summary>
	/// The speech model reports the speech capability and its voices.
	/// </summary>
	[Fact]
	public async Task Show_SpeechModel_ListsVoices()
	{
		var response = await OllamaClient.Models.ShowAsync(new ShowModelRequest { Model = SpeechModel }, CancellationToken);

		response.Capabilities.Should().Contain("speech");
		response.Voices.Should().Contain("default");
	}

	/// <summary>
	/// A decision model answers a choice, a yes/no and a score question in one request.
	/// </summary>
	[Fact]
	public async Task Decide_AnswersEveryQuestionType()
	{
		var response = await OllamaClient.Decisions.DecideAsync(new DecisionRequest
		{
			Model = DecisionModel,
			State = "Our checkout has returned 500 errors since 9am and customers cannot pay.",
			Questions = new()
			{
				["label"] = new ChoiceQuestion
				{
					Instructions = "Which label fits this ticket?",
					Criteria = new() { ["billing"] = "Payments and refunds", ["bug"] = "Software errors" }
				},
				["outage"] = new NoulQuestion { Instructions = "Is a service unavailable?" },
				["urgency"] = new ScoreQuestion { Instructions = "How urgent is this?", Criteria = ["Routine", "Soon", "Immediate"] }
			}
		}, CancellationToken);

		response.Answers["label"].Should().BeOfType<ChoiceAnswer>().Which.Probabilities.Values.Sum().Should().BeApproximately(1, 0.001);
		response.Answers["outage"].Should().BeOfType<NoulAnswer>().Which.Noul.Should().BeInRange(0, 1);
		response.Answers["urgency"].Should().BeOfType<ScoreAnswer>().Which.Score.Should().BeInRange(0, 2);
		response.Usage!.InputTokens.Should().BePositive();
	}

	/// <summary>
	/// Embedding two texts returns two vectors of the requested length.
	/// </summary>
	[Fact]
	public async Task Embed_ReturnsOneVectorPerInput()
	{
		var response = await OllamaClient.Embeddings.EmbedAsync(new EmbedRequest
		{
			Model = TestModels.GetModelName(ModelType.NomicEmbedText),
			Input = ["hello", "world"],
			Dimensions = 64
		}, CancellationToken);

		response.Embeddings.Should().HaveCount(2).And.AllSatisfy(v => v.Should().HaveCount(64));
	}

	/// <summary>
	/// A model copied to a scratch name can be deleted, and is then gone.
	/// </summary>
	[Fact]
	public async Task Delete_RemovesACopiedModel()
	{
		var scratch = $"ollama-api-test-{Guid.NewGuid():N}:delete";
		await OllamaClient.Models.CopyAsync(
			new CopyModelRequest { Source = TestModels.GetModelName(ModelType.NomicEmbedText), Destination = scratch },
			CancellationToken);

		var response = await OllamaClient.Models.DeleteAsync(new DeleteModelRequest { Model = scratch }, CancellationToken);

		response.Status.Should().Be("success");
		(await OllamaClient.Models.GetTagsAsync(CancellationToken)).Models!.Select(m => m.Name).Should().NotContain(scratch);
	}

	/// <summary>
	/// A blob the server holds is found by its digest, and a made-up digest is not.
	/// </summary>
	[Fact]
	public async Task BlobExists_FindsAHeldBlob()
	{
		var tags = await OllamaClient.Models.GetTagsAsync(CancellationToken);
		var digest = tags.Models!.First(m => m.Name!.StartsWith("nomic-embed-text", StringComparison.Ordinal)).Digest;

		(await OllamaClient.Models.BlobExistsAsync($"sha256:{digest}", CancellationToken)).Should().BeTrue();
		(await OllamaClient.Models.BlobExistsAsync($"sha256:{new string('0', 64)}", CancellationToken)).Should().BeFalse();
	}

	/// <summary>
	/// A thinking model returns its reasoning from generate, and the loaded model is listed with its memory use.
	/// </summary>
	[Fact]
	public async Task Generate_Think_ReturnsReasoning()
	{
		var response = await OllamaClient.Generate.GenerateAsync(new GenerateRequest
		{
			Model = TestModel,
			Prompt = "What is 2+2? Answer briefly.",
			Think = true,
			Options = new GenerateOptions { NumPredict = 400 }
		}, CancellationToken);

		response.Thinking.Should().NotBeNullOrWhiteSpace();
		response.PromptEvalCount.Should().BePositive();

		var loaded = await OllamaClient.Utility.GetPsAsync(CancellationToken);
		loaded.Models.Should().Contain(m => m.Name == TestModel && m.SizeVram > 0);
	}
}
