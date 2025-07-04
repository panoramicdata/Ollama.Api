using AwesomeAssertions;
using Ollama.Api.Models;
using Xunit.Abstractions;

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

		var response = await OllamaClient.Chat.ChatAsync(request, default);
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

		var response = await OllamaClient.Chat.ChatAsync(request, CancellationToken.None);
		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message!.Content.Should().Contain("Germany");
		response.Message!.Content.Should().Contain("Berlin");
	}

	[Fact]
	public async Task ToolUse_Succeeds()
	{
		var request = new ChatRequest
		{
			Model = "hengwen/watt-tool-8B",
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
						Name = "get_current_city_weather",
						Description = "Get the current weather for the specified city",
						InputSchema = new ChatToolFunctionInputSchema
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

		var response = await OllamaClient
			.Chat
			.ChatAsync(request, CancellationToken.None);

		response.Should().NotBeNull();
		response.Message.Should().NotBeNull();
		response.Message.Content.Should().BeEmpty();
		response.Message.ToolCalls.Should().HaveCount(1);
		response.Message.ToolCalls[0].Function.Should().NotBeNull();
		response.Message.ToolCalls[0].Function!.Name.Should().Be("get_current_city_weather");
		response.Message.ToolCalls[0].Function.Arguments.Should().NotBeNull();
		response.Message.ToolCalls[0].Function.Arguments.Should().HaveCount(2);
		response.Message.ToolCalls[0].Function.Arguments["city"].Should().Be("Paris");
		response.Message.ToolCalls[0].Function.Arguments["unit"].Should().Be("Celsius");
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
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Chat.ChatAsync(request, default);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
