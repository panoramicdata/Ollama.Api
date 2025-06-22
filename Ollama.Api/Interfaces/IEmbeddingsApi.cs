using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// The Ollama Embeddings API endpoints
/// </summary>
public interface IEmbeddingsApi
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
