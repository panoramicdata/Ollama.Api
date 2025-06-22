using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to generate text using a specified model and prompt.
/// </summary>
public class GenerateRequest
{
	/// <summary>
	/// The name of the model to use for generation.
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// The prompt to provide to the model.
	/// </summary>
	[JsonPropertyName("prompt")]
	public required string Prompt { get; set; }

	/// <summary>
	/// Whether to stream the response as it is generated.
	/// </summary>
	[JsonPropertyName("stream")]
	public required bool Stream { get; set; }

	/// <summary>
	/// Additional generation options such as temperature, top_k, etc.
	/// </summary>
	[JsonPropertyName("options")]
	public GenerateOptions? Options { get; set; }

	/// <summary>
	/// The output format (e.g., "json").
	/// </summary>
	[JsonPropertyName("format")]
	public string? Format { get; set; }

	/// <summary>
	/// If true, the prompt is sent directly without templating.
	/// </summary>
	[JsonPropertyName("raw")]
	public bool? Raw { get; set; }

	/// <summary>
	/// Text to append after the model's response.
	/// </summary>
	[JsonPropertyName("suffix")]
	public string? Suffix { get; set; }
}
