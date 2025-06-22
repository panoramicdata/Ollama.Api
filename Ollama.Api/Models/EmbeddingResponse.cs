using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Response from the embeddings API.
/// </summary>
public class EmbeddingResponse
{
	/// <summary>
	/// The list of embedding vectors.
	/// </summary>
	[JsonPropertyName("embedding")]
	public required float[] Embeddings { get; init; }
}
