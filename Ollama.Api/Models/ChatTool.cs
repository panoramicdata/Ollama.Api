using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a tool definition for the chat API.
/// </summary>
public class ChatTool
{
	[JsonPropertyName("type")]
	public required McpType Type { get; set; }

	[JsonPropertyName("function")]
	public required ChatToolFunction Function { get; set; }
}
