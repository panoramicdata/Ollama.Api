using AwesomeAssertions;
using Ollama.Api.Models;
using Xunit.Abstractions;

namespace Ollama.Api.Test;

public class EmbeddingTests(Fixture fixture, ITestOutputHelper testOutputHelper) : Test(fixture, testOutputHelper)
{
	[Fact]
	public async Task BasicEmbeddings_Succeeds()
	{
		// Act
		var response = await OllamaClient.Embeddings.GetEmbeddingsAsync(new EmbeddingRequest
		{
			Model = "nomic-embed-text",
			Prompt = "Hello, how are you?"
		}, default);

		// Assert
		response.Should().NotBeNull();
		response.Embeddings.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task Embeddings_MissingModel_Returns404()
	{
		var request = new EmbeddingRequest
		{
			Model = "not-a-real-model:fake",
			Prompt = "test"
		};
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Embeddings.GetEmbeddingsAsync(request, default);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
