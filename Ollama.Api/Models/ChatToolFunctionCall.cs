using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

public class ChatToolFunctionCall
{
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	[JsonPropertyName("arguments")]
	public required Dictionary<string, object?> Arguments { get; set; }
}