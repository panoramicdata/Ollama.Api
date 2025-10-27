using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

public class ChatTests(ITestOutputHelper testOutputHelper, Fixture fixture)
: Test(fixture, testOutputHelper)
{
	[Fact]
	public async Task BasicChatCompletion_Succeeds()
	{
		var request = new ChatRequest
		{
			Model = TestModel,
			Messages =
			[
				new ChatMessage { Role = "user", Content = "Hello, who are you?" }
			],
			Stream = false
		};

		var response = await OllamaClient.Chat.ChatAsync(request, CancellationToken);
		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message!.Content.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task MultiTurnChatCompletion_Succeeds()
	{
		var request = new ChatRequest
		{
			Model = TestModel,
			Messages =
			[
				new ChatMessage { Role = "user", Content = "What is the capital of France?" },
				new ChatMessage { Role = "assistant", Content = "The capital of France is Paris." },
				new ChatMessage { Role = "user", Content = "And what about Germany?" }
			],
			Stream = false
		};

		var response = await OllamaClient.Chat.ChatAsync(request, CancellationToken);
		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message!.Content.Should().Contain("Germany");
		response.Message!.Content.Should().Contain("Berlin");
	}

	[Fact]
	public async Task ToolUse_Succeeds()
	{
		var request = CreateWeatherToolRequest();

		var response = await OllamaClient
			.Chat
			.ChatAsync(request, CancellationToken);

		AssertWeatherToolResponse(response);
	}

	private static ChatRequest CreateWeatherToolRequest()
	{
		return new ChatRequest
		{
			Model = TestModels.GetModelName(ModelType.Llama31),
			Messages =
			[
				new ChatMessage { Role = "user", Content = "What is the temperature in Paris right now?" }
			],
			Stream = false,
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
										Description = "The unit to return.  May be Celsius or Fahrenheit."
									}
								}
							},
							Required = [ "city", "unit" ]
						}
					}
				}
			]
		};
	}

	private static void AssertWeatherToolResponse(ChatResponse response)
	{
		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message.Content.Should().BeEmpty();
		response.Message.ToolCalls.Should().ContainSingle();
		response.Message.ToolCalls[0].Function.Should().NotBeNull();
		response.Message.ToolCalls[0].Function!.Name.Should().Be("get_current_weather");
		response.Message.ToolCalls[0].Function.Arguments.Should().NotBeNull();
		response.Message.ToolCalls[0].Function.Arguments.Should().HaveCount(2);
		response.Message.ToolCalls[0].Function.Arguments["city"].Should().NotBeNull();
		response.Message.ToolCalls[0].Function.Arguments["city"]!.ToString().Should().Be("Paris");
		response.Message.ToolCalls[0].Function.Arguments["unit"].Should().NotBeNull();
		response.Message.ToolCalls[0].Function.Arguments["unit"]!.ToString().Should().Be("Celsius");
	}

	[Fact]
	public async Task Chat_MissingModel_Returns404()
	{
		var request = new ChatRequest
		{
			Model = "not-a-real-model:fake",
			Messages = [new ChatMessage { Role = "user", Content = "test" }],
			Stream = false
		};

		// Act
		var act = async () => await OllamaClient.Chat.ChatAsync(request, CancellationToken);

		// Assert
		var exception = await act.Should().ThrowAsync<Refit.ApiException>();
		exception.Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
