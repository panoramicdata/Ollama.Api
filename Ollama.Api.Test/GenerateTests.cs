using AwesomeAssertions;
using Ollama.Api.Models;
using Xunit.Abstractions;

namespace Ollama.Api.Test;

public class GenerateTests(Fixture fixture, ITestOutputHelper testOutputHelper) : Test(fixture, testOutputHelper)
{
	private const string LlavaModel = "llava:latest";

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

	[Fact]
	public async Task Generate_DescribeImage_Succeeds()
	{
		// Arrange
		var imageFileInfo = new FileInfo("Images/Ollama Logo.png");
		imageFileInfo.Exists.Should().BeTrue("Test image file should exist for this test to run.");
		var imageBytes = await File.ReadAllBytesAsync(imageFileInfo.FullName);
		var base64EncodedImage = Convert.ToBase64String(imageBytes);

		// Make sure the model supports image input
		var tagsResponse = await OllamaClient.Models.GetTagsAsync(default);
		tagsResponse.Should().NotBeNull();
		var modelSupportsImages = tagsResponse.Models?.Any(m => string.Equals(m.Name, LlavaModel, StringComparison.OrdinalIgnoreCase)) ?? false;
		modelSupportsImages.Should().BeTrue("The 'llava' model should be available for this test to run.");

		// Act
		var request = new GenerateRequest
		{
			Model = LlavaModel,
			Prompt = "Describe this image.",
			Images = [base64EncodedImage],
			Stream = false
		};

		var response = await OllamaClient.Generate.GenerateAsync(request, default);

		// Assert
		response.Should().NotBeNull();
		response.Model.Should().Be(LlavaModel);
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
}
