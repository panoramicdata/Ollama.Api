using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

/// <summary>
/// Exercises the /api/chat endpoint against a live Ollama server.
/// </summary>
/// <param name="testOutputHelper">Where log output for the test is written.</param>
/// <param name="fixture">The shared fixture that supplies configuration and services.</param>
public class ChatTests(ITestOutputHelper testOutputHelper, Fixture fixture)
: Test(fixture, testOutputHelper)
{
	/// <summary>
	/// A single-turn chat returns a non-empty assistant message.
	/// </summary>
	[Fact]
	public async Task BasicChatCompletion_Succeeds()
	{
		var request = CreateBasicChatRequest("Hello, who are you?");

		var response = await OllamaClient.Chat.ChatAsync(request, CancellationToken);
		AssertBasicChatResponse(response);
	}

	/// <summary>
	/// A conversation carrying earlier turns returns a reply that takes them into account.
	/// </summary>
	[Fact]
	public async Task MultiTurnChatCompletion_Succeeds()
	{
		var request = CreateBasicChatRequest(
			new ChatMessage { Role = "user", Content = "What is the capital of France?" },
			new ChatMessage { Role = "assistant", Content = "The capital of France is Paris." },
			new ChatMessage { Role = "user", Content = "And what about Germany?" });

		var response = await OllamaClient.Chat.ChatAsync(request, CancellationToken);
		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message!.Content.Should().Contain("Germany");
		response.Message!.Content.Should().Contain("Berlin");
	}

	/// <summary>
	/// A model offered a weather tool calls it, where the model supports tool use.
	/// </summary>
	/// <param name="modelType">The model under test.</param>
	/// <param name="supportsTool">Whether that model is expected to be able to call a tool.</param>
	[Theory]
	[InlineData(ModelType.Llama3Latest, false)]
	[InlineData(ModelType.LlavaLatest, false)]
	[InlineData(ModelType.Llama3, false)]
	[InlineData(ModelType.NomicEmbedText, false)]
	[InlineData(ModelType.Qwen352b, true)]
	[InlineData(ModelType.Gemma4E2b, true)]
	[InlineData(ModelType.Gemma4E4b, true)]
	public async Task ToolUse_Succeeds(ModelType modelType, bool supportsTool)
	{
		var modelName = TestModels.GetModelName(modelType);

		var request = CreateWeatherToolRequest(modelName);

		var response = await OllamaClient
			.Chat
			.ChatAsync(request, CancellationToken);

		if (supportsTool)
		{
			response.Model.Should().Contain(modelName);
			response.Done.Should().BeTrue();
			response.Message.Should().NotBeNull();
			response.Message.ToolCalls.Should().NotBeEmpty();
			response.Message.Thinking.Should().NotBeNull();
		}
		else
		{
			response.Done.Should().BeFalse();
			response.Message.Should().BeNull();
			response.Error.Should().NotBeNullOrWhiteSpace();
			response.Error.Should().Contain("does not support");
			return;
		}

		AssertWeatherToolResponse(response);
	}

	private static ChatRequest CreateWeatherToolRequest(string modelName) => new()
	{
		Model = modelName,
		Messages =
			[
				new ChatMessage { Role = "user", Content = "What is the temperature in Paris right now?" }
			],
		Stream = false,
		Format = null,
		KeepAlive = null,
		Options = new GenerateOptions {
		},
		Tools =
			[
				new() {
					Type = McpType.Function,
					Function = new ChatToolFunction
					{
						Name = "get_current_weather",
						Description = "Get the current weather for a specified city. Use this when a user asks about the weather or temperature.",
						Parameters = new ChatToolFunctionParameters
						{
							Type = McpType.Object,
							Properties = new Dictionary<string, ChatToolFunctionInputSchemaProperty>
							{
								{
									"city",
									new ChatToolFunctionInputSchemaProperty
									{
										Type = McpType.String,
										Description = "The name of the city to get the weather for",
									}
								},
								{
									"unit",
									new ChatToolFunctionInputSchemaProperty
									{
										Type = McpType.String,
										Description = "The unit to return.  May be Celsius or Fahrenheit.  Must be provided. Prefer Celcius if the user does not specify.",
									}
								}
							},
							Required = [ "city", "unit" ]
						}
					}
				}
			]
	};

	private static void AssertWeatherToolResponse(ChatResponse response)
	{
		response.Should().NotBeNull();
		response.Error.Should().BeNull();
		response.Message.Should().NotBeNull();

		if (response.Message.ToolCalls is null || response.Message.ToolCalls.Count == 0)
		{
			response.Message.Content.Should().Contain("Paris");
			return;
		}

		response.Message.ToolCalls.Should().ContainSingle();
		response.Message.ToolCalls[0].Function.Should().NotBeNull();
		response.Message.ToolCalls[0].Function!.Name.Should().Be("get_current_weather");
		response.Message.ToolCalls[0].Function.Arguments.Should().NotBeNull();
		response.Message.ToolCalls[0].Function.Arguments.Should().HaveCount(2);
		response.Message.ToolCalls[0].Function.Arguments["city"].Should().NotBeNull();
		response.Message.ToolCalls[0].Function.Arguments["city"]!.ToString().Should().Be("Paris");
		response.Message.ToolCalls[0].Function.Arguments["unit"].Should().NotBeNull();
	}

	/// <summary>
	/// Chatting to a model that is not installed reports the failure on the response rather than
	/// throwing.
	/// </summary>
	[Fact]
	public async Task Chat_MissingModel_ReturnsErrorInResponse()
	{
		var request = new ChatRequest
		{
			Model = "not-a-real-model:fake",
			Messages = [new ChatMessage { Role = "user", Content = "test" }],
			Stream = false
		};

		var response = await OllamaClient.Chat.ChatAsync(request, CancellationToken);

		response.Error.Should().NotBeNullOrWhiteSpace();
	}

	private static ChatRequest CreateBasicChatRequest(string userMessage)
		=> CreateBasicChatRequest(new ChatMessage { Role = "user", Content = userMessage });

	private static ChatRequest CreateBasicChatRequest(params ChatMessage[] messages)
		=> new()
		{
			Model = TestModel,
			Messages = [.. messages],
			Stream = false
		};

	private static void AssertBasicChatResponse(ChatResponse response)
	{
		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message!.Content.Should().NotBeNullOrWhiteSpace();
	}
}
