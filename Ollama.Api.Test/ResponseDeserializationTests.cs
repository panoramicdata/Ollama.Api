using AwesomeAssertions;
using Ollama.Api.Models;
using System.Text.Json;

namespace Ollama.Api.Test;

public class ResponseDeserializationTests(Fixture fixture, ITestOutputHelper testOutputHelper) : Test(fixture, testOutputHelper)
{
	[Fact]
	public async Task Qwen352b_Response_Deserializes()
	{
		var responseJson = """
{
	"model":"qwen3.5:2b",
	"created_at":"2026-06-02T16:11:26.030991975Z",
	"message":{
		"role":"assistant",
		"content":"",
		"thinking":"The user is asking about the current temperature in Paris. I need to use the get_current_weather function to get this information. I need to provide:\n- city: \"Paris\"\n- unit: The user didn't specify a unit, so I should probably ask or use the default. Looking at the function description, it says \"If not provided, will return Celsius by default.\" Since the user didn't specify they want Fahrenheit, I'll use the default Celsius.\n\nLet me make the function call.",
		"tool_calls":[
			{
				"id":"call_09rcvydw",
				"function":{
					"index":0,
					"name":"get_current_weather",
					"arguments":{
						"city":"Paris"
					}
				}
			}
		]
	},
	"done":true,
	"done_reason":"stop",
	"total_duration":1453270195,
	"load_duration":210699834,
	"prompt_eval_count":347,
	"prompt_eval_duration":73920373,
	"eval_count":133,
	"eval_duration":997215407
}
""";

		// Act

		// Deserialize the response using the same serializer options as the API client
		var result = JsonSerializer.Deserialize<ChatResponse>(responseJson, OllamaClient.JsonSerializerOptions);

		// Assert
		result.Should().NotBeNull();
		result.Model.Should().Be("qwen3.5:2b");
		result.CreatedAt.Should().Be(DateTimeOffset.Parse("2026-06-02T16:11:26.0309919Z"));

		result.Message.Should().NotBeNull();

		result.Message!.Role.Should().Be("assistant");

		result.Message.Content.Should().Be("");

		result.Message.Thinking.Should().NotBeNull();
		result.Message.Thinking.Should().Be("The user is asking about the current temperature in Paris. I need to use the get_current_weather function to get this information. I need to provide:\n- city: \"Paris\"\n- unit: The user didn't specify a unit, so I should probably ask or use the default. Looking at the function description, it says \"If not provided, will return Celsius by default.\" Since the user didn't specify they want Fahrenheit, I'll use the default Celsius.\n\nLet me make the function call.");

		result.Message.ToolCalls.Should().NotBeNull();
		result.Message.ToolCalls!.Should().ContainSingle();
		var toolCall = result.Message.ToolCalls[0];
		toolCall.Id.Should().Be("call_09rcvydw");
		toolCall.Function.Should().NotBeNull();
		toolCall.Function.Name.Should().Be("get_current_weather");
		toolCall.Function.Arguments.Should().NotBeNull();

		var keyPresent = toolCall.Function.Arguments.TryGetValue("city", out var cityValue);
		keyPresent.Should().BeTrue();
		cityValue.Should().NotBeNull();
		var cityValueString = cityValue as string;
		cityValueString.Should().Be("Paris");
	}
}
