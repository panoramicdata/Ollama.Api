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
	public async Task Chat_MissingModel_Returns404()
	{
		var request = new ChatRequest
		{
			Model = "not-a-real-model:fake",
			Messages = [ new ChatMessage { Role = "user", Content = "test" } ],
			Stream = false
		};
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Chat.ChatAsync(request, default);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
