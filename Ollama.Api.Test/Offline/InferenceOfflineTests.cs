using AwesomeAssertions;
using Ollama.Api.Models;
using Refit;
using System.Net;
using System.Text.Json;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// Chat, generate and embed, answered from responses qwen3.5:2b and nomic-embed-text gave Ollama 0.40.13.
/// </summary>
public class InferenceOfflineTests
{
	/// <summary>
	/// A thinking generate returns its reasoning, why it stopped and how much of the prompt was cached.
	/// </summary>
	[Fact]
	public async Task Generate_ReadsThinkingAndCachedTokens()
	{
		var transport = new StubTransport().Then(Respond.Fixture("generate-think.json"));
		using var client = Offline.Client(transport);

		var response = await client.Generate.GenerateAsync(new GenerateRequest
		{
			Model = "qwen3.5:2b",
			Prompt = "What is 2+2? Answer briefly.",
			Think = "high",
			Logprobs = true,
			TopLogprobs = 1
		}, Offline.Token);

		response.Response.Should().Be("4");
		response.Thinking.Should().StartWith("Thinking Process:");
		response.DoneReason.Should().Be(DoneReason.Stop);
		response.PromptEvalCount.Should().Be(20);
		response.PromptEvalCachedCount.Should().Be(16);
		response.Logprobs.Should().BeNull();

		var sent = transport.LastRequest.Json;
		sent["think"]!.GetValue<string>().Should().Be("high");
		sent["logprobs"]!.GetValue<bool>().Should().BeTrue();
		sent["top_logprobs"]!.GetValue<int>().Should().Be(1);
	}

	/// <summary>
	/// A chat with log probabilities returns each token's log probability and its alternatives.
	/// </summary>
	[Fact]
	public async Task Chat_ReadsLogprobs()
	{
		var transport = new StubTransport().Then(Respond.Fixture("chat-think-logprobs.json"));
		using var client = Offline.Client(transport);

		var response = await client.Chat.ChatAsync(new ChatRequest
		{
			Model = "qwen3.5:2b",
			Messages = [new ChatMessage { Role = "user", Content = "What is 2+2?" }],
			Think = true,
			Logprobs = true,
			TopLogprobs = 2
		}, Offline.Token);

		response.DoneReason.Should().Be(DoneReason.Length);
		response.PromptEvalCachedCount.Should().Be(0);
		var first = response.Logprobs.Should().HaveCount(3).And.Subject.First();
		first.Token.Should().Be("Thinking");
		first.Logprob.Should().BeNegative();
		first.Bytes.Should().Equal(84, 104, 105, 110, 107, 105, 110, 103);
		first.TopLogprobs.Should().HaveCount(2);
		first.TopLogprobs![0].Token.Should().Be("Thinking");
	}

	/// <summary>
	/// A failed chat carries Ollama's own message in the response rather than throwing.
	/// </summary>
	[Fact]
	public async Task Chat_Failure_CarriesTheMessage()
	{
		var transport = new StubTransport().Then(Respond.Json(HttpStatusCode.NotFound, """{"error":"model \"x\" not found"}"""));
		using var client = Offline.Client(transport);

		var response = await client.Chat.ChatAsync(new ChatRequest { Model = "x", Messages = [] }, Offline.Token);

		response.Error.Should().Be("model \"x\" not found");
	}

	/// <summary>
	/// A failed chat with no body falls back to the status's reason phrase.
	/// </summary>
	[Fact]
	public async Task Chat_FailureWithoutBody_UsesTheReasonPhrase()
	{
		var transport = new StubTransport().Then(Respond.Empty(HttpStatusCode.BadGateway));
		using var client = Offline.Client(transport);

		var response = await client.Chat.ChatAsync(new ChatRequest { Model = "x", Messages = [] }, Offline.Token);

		response.Error.Should().Be("Bad Gateway");
	}

	/// <summary>
	/// A chat to an unreachable server carries the transport's message rather than throwing.
	/// </summary>
	[Fact]
	public async Task Chat_Unreachable_CarriesTheTransportMessage()
	{
		var transport = new StubTransport().ThenUnreachable();
		using var client = Offline.Client(transport);

		var response = await client.Chat.ChatAsync(new ChatRequest { Model = "x", Messages = [] }, Offline.Token);

		response.Error.Should().Contain("Connection refused");
	}

	/// <summary>
	/// A streamed chat yields each chunk until the done chunk, skipping blank and null lines.
	/// </summary>
	[Fact]
	public async Task ChatStream_YieldsChunksUntilDone()
	{
		const string Stream = "{\"message\":{\"role\":\"assistant\",\"content\":\"Hel\"},\"done\":false}\n\nnull\n"
			+ "{\"message\":{\"role\":\"assistant\",\"content\":\"lo\"},\"done\":true,\"done_reason\":\"stop\"}\n"
			+ "{\"message\":{\"role\":\"assistant\",\"content\":\"after\"},\"done\":false}\n";
		var transport = new StubTransport().Then(Respond.Streamed("application/x-ndjson", Stream));
		using var client = Offline.Client(transport);

		var chunks = await ToListAsync(client.Chat.ChatStreamAsync(new ChatRequest { Model = "x", Messages = [], Think = "low" }, Offline.Token));

		chunks.Select(c => c.Message!.Content).Should().Equal("Hel", "lo");
		transport.LastRequest.Json["stream"]!.GetValue<bool>().Should().BeTrue();
		transport.LastRequest.Json["think"]!.GetValue<string>().Should().Be("low");
	}

	/// <summary>
	/// A malformed chunk ends the stream with an error rather than a silent gap.
	/// </summary>
	[Fact]
	public async Task ChatStream_MalformedChunk_EndsWithAnError()
	{
		var transport = new StubTransport().Then(Respond.Streamed("application/x-ndjson", "{not json}\n"));
		using var client = Offline.Client(transport);

		var chunks = await ToListAsync(client.Chat.ChatStreamAsync(new ChatRequest { Model = "x", Messages = [] }, Offline.Token));

		chunks.Should().ContainSingle().Which.Error.Should().StartWith("Could not parse a streamed chat response");
	}

	/// <summary>
	/// A refused streamed chat yields one response carrying the message, from either error shape.
	/// </summary>
	/// <param name="body">The error body.</param>
	/// <param name="expected">The message the response should carry.</param>
	[Theory]
	[InlineData("""{"error":"model \"x\" not found"}""", "model \"x\" not found")]
	[InlineData("""{"error":{"message":"bad request"}}""", "bad request")]
	[InlineData("", "Not Found")]
	public async Task ChatStream_Refused_YieldsTheError(string body, string expected)
	{
		var transport = new StubTransport().Then(Respond.Json(HttpStatusCode.NotFound, body));
		using var client = Offline.Client(transport);

		var chunks = await ToListAsync(client.Chat.ChatStreamAsync(new ChatRequest { Model = "x", Messages = [] }, Offline.Token));

		chunks.Should().ContainSingle().Which.Error.Should().Be(expected);
	}

	/// <summary>
	/// A missing chat request is refused before anything is sent.
	/// </summary>
	[Fact]
	public void ChatStream_NullRequest_IsRefused()
	{
		using var client = Offline.Client(new StubTransport());

		var act = () => client.Chat.ChatStreamAsync(null!, Offline.Token);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// A JSON schema format is sent as an object, not as a string.
	/// </summary>
	[Fact]
	public void Format_Schema_IsSentAsAnObject()
	{
		var json = JsonSerializer.SerializeToNode(
			new ChatRequest { Model = "x", Messages = [], Format = new { type = "object" } },
			OllamaClient.JsonSerializerOptions)!;

		json["format"]!["type"]!.GetValue<string>().Should().Be("object");
	}

	/// <summary>
	/// Embedding several texts returns one vector each, of the requested length.
	/// </summary>
	[Fact]
	public async Task Embed_ReturnsOneVectorPerInput()
	{
		var transport = new StubTransport().Then(Respond.Fixture("embed.json"));
		using var client = Offline.Client(transport);

		var response = await client.Embeddings.EmbedAsync(new EmbedRequest
		{
			Model = "nomic-embed-text",
			Input = ["hello", "world"],
			Dimensions = 8,
			Truncate = true,
			KeepAlive = "1m",
			Options = new GenerateOptions { NumCtx = 512 }
		}, Offline.Token);

		response.Model.Should().Be("nomic-embed-text");
		response.Embeddings.Should().HaveCount(2).And.AllSatisfy(v => v.Should().HaveCount(8));
		response.PromptEvalCount.Should().Be(6);
		response.TotalDuration.Should().BePositive();
		response.LoadDuration.Should().BePositive();
		transport.LastRequest.Uri.AbsolutePath.Should().Be("/api/embed");
		transport.LastRequest.Json["dimensions"]!.GetValue<int>().Should().Be(8);
	}

	/// <summary>
	/// The older embeddings endpoint is still reachable.
	/// </summary>
	[Fact]
	public async Task GetEmbeddings_PostsToTheOlderEndpoint()
	{
		var transport = new StubTransport().Then(Respond.Json("""{"embedding":[0.5,0.25]}"""));
		using var client = Offline.Client(transport);

		var response = await client.Embeddings.GetEmbeddingsAsync(new EmbeddingRequest { Model = "m", Prompt = "p" }, Offline.Token);

		response.Embeddings.Should().Equal(0.5f, 0.25f);
		transport.LastRequest.Uri.AbsolutePath.Should().Be("/api/embeddings");
	}

	/// <summary>
	/// An embeddings request needs exactly one of a prompt and an input.
	/// </summary>
	/// <param name="hasPrompt">Whether the request has a prompt.</param>
	/// <param name="hasInput">Whether the request has an input.</param>
	/// <param name="valid">Whether that combination is valid.</param>
	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(true, true, false)]
	[InlineData(false, false, false)]
	public void EmbeddingRequest_NeedsExactlyOneOfPromptAndInput(bool hasPrompt, bool hasInput, bool valid)
	{
		var request = new EmbeddingRequest
		{
			Model = "m",
			Prompt = hasPrompt ? "p" : null,
			Input = hasInput ? ["i"] : null
		};

		var act = request.Validate;

		if (valid)
		{
			act.Should().NotThrow();
		}
		else
		{
			act.Should().Throw<ArgumentException>();
		}
	}

	/// <summary>
	/// A think value is written as the boolean or level it holds, and read back the same.
	/// </summary>
	[Fact]
	public void ThinkValue_RoundTrips()
	{
		Serialize(true).Should().Be("true");
		Serialize(false).Should().Be("false");
		Serialize("max").Should().Be("\"max\"");
		Serialize(default).Should().Be("false");

		JsonSerializer.Deserialize<ThinkValue>("\"low\"", OllamaClient.JsonSerializerOptions).Level.Should().Be("low");
		JsonSerializer.Deserialize<ThinkValue>("false", OllamaClient.JsonSerializerOptions).Enabled.Should().BeFalse();
		ThinkValue.FromLevel("high").ToString().Should().Be("high");
		ThinkValue.FromBoolean(true).ToString().Should().Be("true");
		ThinkValue.FromBoolean(false).ToString().Should().Be("false");
	}

	/// <summary>
	/// A think value that is neither a boolean nor a string is rejected, as is a blank level.
	/// </summary>
	[Fact]
	public void ThinkValue_Invalid_IsRejected()
	{
		var read = () => JsonSerializer.Deserialize<ThinkValue>("3", OllamaClient.JsonSerializerOptions);
		var readNull = () => JsonSerializer.Deserialize<ThinkValue>("null", OllamaClient.JsonSerializerOptions);
		var blank = () => ThinkValue.FromLevel(" ");

		read.Should().Throw<JsonException>();
		readNull.Should().Throw<JsonException>();
		blank.Should().Throw<ArgumentException>();
	}

	private static string Serialize(ThinkValue value) => JsonSerializer.Serialize(value, OllamaClient.JsonSerializerOptions);

	private static async Task<List<ChatResponse>> ToListAsync(IAsyncEnumerable<ChatResponse> chunks)
	{
		var list = new List<ChatResponse>();
		await foreach (var chunk in chunks)
		{
			list.Add(chunk);
		}

		return list;
	}
}
