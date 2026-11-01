using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Generates numerical vector representations (embeddings) for input text.
/// </summary>
public interface IEmbeddings
{
	/// <summary>
	/// Gets one embedding per input text from <c>/api/embed</c>, normalised to unit length.
	/// </summary>
	/// <param name="request">The model and the texts to embed.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("/api/embed")]
	Task<EmbedResponse> EmbedAsync(
		EmbedRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets embeddings for the given input(s) using the specified model, from the older
	/// <c>/api/embeddings</c> endpoint. Prefer <see cref="EmbedAsync"/>.
	/// </summary>
	/// <param name="request">The embedding request containing model and input(s).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation, containing the embedding response.</returns>
	[Post("/api/embeddings")]
	Task<EmbeddingResponse> GetEmbeddingsAsync(
		EmbeddingRequest request,
		CancellationToken cancellationToken);
}
