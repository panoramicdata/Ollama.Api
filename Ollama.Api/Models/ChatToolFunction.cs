using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

public class ChatToolFunction
{
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("inputSchema")]
	public ChatToolFunctionInputSchema? InputSchema { get; set; }
}
