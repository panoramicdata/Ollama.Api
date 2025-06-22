using AwesomeAssertions;
using Ollama.Api.Models;
using Xunit.Abstractions;

namespace Ollama.Api.Test;

public class GenerateTests(Fixture fixture, ITestOutputHelper testOutputHelper) : Test(fixture, testOutputHelper)
{
	[Fact]
	public async Task MinimalGenerate_Succeeds()
	{
		// Act
		var response = await OllamaClient.Generate.GenerateAsync(new GenerateRequest
		{
			Model = "llama3",
			Prompt = "Hello, how are you?",
			Stream = false
		}, default);

		// Assert
		response.Should().NotBeNull();
		response.Model.Should().Be("llama3");
		response.Response.Should().NotBeNullOrEmpty();
		response.Done.Should().BeTrue();
		response.Context.Should().NotBeNull();
		response.Context!.Count.Should().BePositive();
		response.TotalDuration.Should().BePositive();
		response.LoadDuration.Should().BePositive();
		response.PromptEvalCount.Should().BePositive();
		response.PromptEvalDuration.Should().BePositive();
		response.CreatedAt.Should().NotBeNull();
		response.CreatedAt!.Value.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
	}

	[Fact]
	public async Task Generate_MissingModel_Returns404()
	{
		var request = new GenerateRequest
		{
			Model = "not-a-real-model:fake",
			Prompt = "test",
			Stream = false
		};
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Generate.GenerateAsync(request, default);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
