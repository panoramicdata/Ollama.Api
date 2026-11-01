using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to fetch detailed information about a specific local model.
/// </summary>
public class ShowModelRequest : ModelRequest
{
	/// <summary>
	/// Whether to include the large fields, such as the tokenizer's vocabulary, in the model information.
	/// </summary>
	[JsonPropertyName("verbose")]
	public bool? Verbose { get; set; }
}
