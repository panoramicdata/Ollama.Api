using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to fetch detailed information about a specific local model.
/// </summary>
public class ShowModelRequest
{
	/// <summary>
	/// The full name of the model to show, including its tag (e.g., "llama3.1:latest").
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }
}
