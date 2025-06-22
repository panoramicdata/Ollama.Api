using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response containing detailed information about a specific local model.
/// </summary>
public class ShowModelResponse
{
	/// <summary>
	/// The name of the model.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// The size of the model in bytes.
	/// </summary>
	[JsonPropertyName("size")]
	public long? Size { get; set; }

	/// <summary>
	/// The last modified timestamp of the model.
	/// </summary>
	[JsonPropertyName("modified_at")]
	public DateTimeOffset? ModifiedAt { get; set; }

	/// <summary>
	/// Additional details about the model (structure may vary).
	/// </summary>
	[JsonPropertyName("details")]
	public object? Details { get; set; } // Use a more specific type if available
}
