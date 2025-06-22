using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Options for controlling the text generation process.
/// </summary>
public class GenerateOptions
{
	/// <summary>
	/// Sampling temperature. Higher values increase randomness.
	/// </summary>
	[JsonPropertyName("temperature")]
	public double? Temperature { get; set; }

	/// <summary>
	/// Limits the next token selection to the top_k most probable tokens.
	/// </summary>
	[JsonPropertyName("top_k")]
	public int? TopK { get; set; }

	/// <summary>
	/// Nucleus sampling probability threshold.
	/// </summary>
	[JsonPropertyName("top_p")]
	public double? TopP { get; set; }

	/// <summary>
	/// Maximum number of tokens to predict.
	/// </summary>
	[JsonPropertyName("num_predict")]
	public int? NumPredict { get; set; }

	/// <summary>
	/// Random seed for reproducible outputs.
	/// </summary>
	[JsonPropertyName("seed")]
	public int? Seed { get; set; }

	/// <summary>
	/// Context window size.
	/// </summary>
	[JsonPropertyName("num_ctx")]
	public int? NumCtx { get; set; }
}
