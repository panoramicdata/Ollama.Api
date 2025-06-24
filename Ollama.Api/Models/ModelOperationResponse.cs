using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a generic response for model management operations (copy, delete, pull, push, create).
/// </summary>
public class ModelOperationResponse
{
	/// <summary>
	/// Any operation errors
	/// </summary>
	[JsonPropertyName("error")]
	public string? Error { get; set; }

	/// <summary>
	/// The status of the operation (e.g., "success", "error").
	/// </summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }
}
