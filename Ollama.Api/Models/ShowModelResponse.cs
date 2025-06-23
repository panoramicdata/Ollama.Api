using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response containing detailed information about a specific local model.
/// </summary>
public class ShowModelResponse
{
	/// <summary>
	/// The last modified timestamp of the model.
	/// </summary>
	[JsonPropertyName("modified_at")]
	public required DateTimeOffset ModifiedAt { get; set; }

	/// <summary>
	/// Additional details about the model (structure may vary).
	/// </summary>
	[JsonPropertyName("details")]
	public required ModelDetails Details { get; set; } // Use a more specific type if available
}