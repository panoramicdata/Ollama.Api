using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The embeddings returned by <c>/api/embed</c>.
/// </summary>
public class EmbedResponse
{
	/// <summary>
	/// The model that produced the embeddings.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// One vector per input text, in request order.
	/// </summary>
	[JsonPropertyName("embeddings")]
	public List<float[]> Embeddings { get; set; } = [];

	/// <summary>
	/// Total time spent, in nanoseconds.
	/// </summary>
	[JsonPropertyName("total_duration")]
	public long? TotalDuration { get; set; }

	/// <summary>
	/// Time spent loading the model, in nanoseconds.
	/// </summary>
	[JsonPropertyName("load_duration")]
	public long? LoadDuration { get; set; }

	/// <summary>
	/// Number of input tokens embedded.
	/// </summary>
	[JsonPropertyName("prompt_eval_count")]
	public int? PromptEvalCount { get; set; }
}
