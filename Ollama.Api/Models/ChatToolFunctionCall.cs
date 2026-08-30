using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A model's request to call one of the functions it was offered.
/// </summary>
public class ChatToolFunctionCall
{
	/// <summary>
	/// The name of the function the model wants called.
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// The arguments the model chose, keyed by parameter name.
	/// </summary>
	[JsonPropertyName("arguments")]
	public required Dictionary<string, object?> Arguments { get; set; }
}