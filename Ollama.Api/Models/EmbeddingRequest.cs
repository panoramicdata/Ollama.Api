using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Request for the embeddings API.
/// </summary>
public class EmbeddingRequest
{
	/// <summary>
	/// The model to use for generating embeddings.
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// The input text(s) to embed.
	/// </summary>
	[JsonPropertyName("prompt")]
	public required string Prompt { get; set; }
}
