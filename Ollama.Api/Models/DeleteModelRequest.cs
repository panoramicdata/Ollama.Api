using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to delete a model from local Ollama storage.
/// </summary>
public class DeleteModelRequest
{
	/// <summary>
	/// The full name of the model to delete, including its tag (e.g., "llama3.1:latest").
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }
}
