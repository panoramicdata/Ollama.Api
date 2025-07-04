using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the input schema for a chat tool function with required properties and annotations.
/// </summary>
public class ChatToolFunctionParameters
{
	/// <summary>
	/// The type of the input schema. Should be "object" for tool schemas.
	/// </summary>
	[JsonPropertyName("type")]
	public required McpType Type { get; set; }

	/// <summary>
	/// The properties of the input schema.
	/// </summary>
	[JsonPropertyName("properties")]
	public required Dictionary<string, ChatToolFunctionInputSchemaProperty> Properties { get; set; }

	/// <summary>
	/// The required property names for the input schema.
	/// </summary>
	[JsonPropertyName("required")]
	public required List<string> Required { get; set; }

	/// <summary>
	/// Optional annotations for the input schema.
	/// </summary>
	[JsonPropertyName("annotations")]
	public ChatToolFunctionInputSchemaAnnotations? Annotations { get; set; }
}