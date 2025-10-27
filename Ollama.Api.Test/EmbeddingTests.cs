using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

public class EmbeddingTests(Fixture fixture, ITestOutputHelper testOutputHelper) : Test(fixture, testOutputHelper)
{
	[Fact]
	public async Task BasicEmbeddings_Succeeds()
	{
		// Act
		var response = await OllamaClient.Embeddings.GetEmbeddingsAsync(new EmbeddingRequest
		{
			Model = TestModels.GetModelName(ModelType.NomicEmbedText),
			Prompt = "Hello, how are you?"
		}, CancellationToken);

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
		
		// Act
		var act = async () => await OllamaClient.Embeddings.GetEmbeddingsAsync(request, CancellationToken);
		
		// Assert
		var exception = await act.Should().ThrowAsync<Refit.ApiException>();
		exception.Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}
}
