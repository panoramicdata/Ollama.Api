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
}
