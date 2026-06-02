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

	/// <summary>
	/// The model's internal thoughts that it wants to share with the user. This may include information about what the model is thinking, its plan, or its reasoning process. This field is optional and may not be present in all responses.
	/// </summary>
	[JsonPropertyName("thinking")]
	public string? Thinking { get; set; }
}
