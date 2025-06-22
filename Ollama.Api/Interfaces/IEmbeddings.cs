using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Generates numerical vector representations (embeddings) for input text.
/// </summary>
public interface IEmbeddings
{
	/// <summary>
	/// Gets embeddings for the given input(s) using the specified model.
	/// </summary>
	/// <param name="request">The embedding request containing model and input(s).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation, containing the embedding response.</returns>
	[Post("/api/embeddings")]
	Task<EmbeddingResponse> GetEmbeddingsAsync(
		EmbeddingRequest request,
		CancellationToken cancellationToken = default);
}
