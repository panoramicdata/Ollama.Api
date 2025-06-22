using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a tool call in a chat message.
/// </summary>
public class ChatToolCall
{
	[JsonPropertyName("function")]
	public ChatToolFunction? Function { get; set; }
}
