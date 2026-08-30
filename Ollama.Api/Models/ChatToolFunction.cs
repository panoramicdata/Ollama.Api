using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A function a model may call, as offered to it in a chat request.
/// </summary>
public class ChatToolFunction
{
	/// <summary>
	/// The name the model uses to call the function.
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// What the function does. This is what the model reads when deciding whether to call it, so it
	/// is worth writing properly.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The arguments the function takes, described as a JSON schema.
	/// </summary>
	[JsonPropertyName("parameters")]
	public ChatToolFunctionParameters? Parameters { get; set; }
}
