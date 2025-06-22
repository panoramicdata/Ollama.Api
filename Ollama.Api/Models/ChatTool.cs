using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a tool definition for the chat API.
/// </summary>
public class ChatTool
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("function")]
	public ChatToolFunction? Function { get; set; }
}
