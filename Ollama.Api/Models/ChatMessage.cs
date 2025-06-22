using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a message in a chat conversation.
/// </summary>
public class ChatMessage
{
	/// <summary>
	/// The role of the message sender. Supported roles: "system", "user", "assistant", "tool".
	/// </summary>
	[JsonPropertyName("role")]
	public required string Role { get; set; }

	/// <summary>
	/// The content of the message.
	/// </summary>
	[JsonPropertyName("content")]
	public required string Content { get; set; }

	/// <summary>
	/// A list of base64-encoded images to be included with this message (for multimodal models).
	/// </summary>
	[JsonPropertyName("images")]
	public List<string>? Images { get; set; }

	/// <summary>
	/// Tool calls the model wants to make (for tool use).
	/// </summary>
	[JsonPropertyName("tool_calls")]
	public List<ChatToolCall>? ToolCalls { get; set; }
}
