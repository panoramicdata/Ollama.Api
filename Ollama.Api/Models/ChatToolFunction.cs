using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

public class ChatToolFunction
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("arguments")]
	public object? Arguments { get; set; }
}
