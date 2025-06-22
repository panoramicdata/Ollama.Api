using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to copy an existing model to a new name in Ollama.
/// </summary>
public class CopyModelRequest
{
	/// <summary>
	/// The full name of the model to copy from, including its tag (e.g., "llama3.1:latest").
	/// </summary>
	[JsonPropertyName("source")]
	public required string Source { get; set; }

	/// <summary>
	/// The desired full name for the new copied model, including its tag (e.g., "my-llama3.1:v2").
	/// </summary>
	[JsonPropertyName("destination")]
	public required string Destination { get; set; }
}
