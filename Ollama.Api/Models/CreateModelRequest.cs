using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to create a new custom model in Ollama.
/// </summary>
public class CreateModelRequest
{
	/// <summary>
	/// The full name for the new model to create, including its tag (e.g., "my-custom-model:v1").
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// The base model to use for creation (e.g., "llama3.1:latest").
	/// </summary>
	[JsonPropertyName("from")]
	public required string From { get; set; }

	/// <summary>
	/// A system message to set the persona or instructions for the model (optional).
	/// </summary>
	[JsonPropertyName("system")]
	public string? System { get; set; }

	/// <summary>
	/// The path to the model or Modelfile content (optional).
	/// </summary>
	[JsonPropertyName("path")]
	public string? Path { get; set; }
}
