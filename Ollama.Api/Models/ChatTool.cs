using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a tool definition for the chat API.
/// </summary>
public class ChatTool
{
	/// <summary>
	/// The kind of tool being described. Ollama currently only understands functions.
	/// </summary>
	[JsonPropertyName("type")]
	public required McpType Type { get; set; }

	/// <summary>
	/// The function the model may call: its name, what it does, and the arguments it takes.
	/// </summary>
	[JsonPropertyName("function")]
	public required ChatToolFunction Function { get; set; }
}
