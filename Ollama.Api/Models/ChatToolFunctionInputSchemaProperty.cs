using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a property in the input schema for a chat tool function.
/// </summary>
public class ChatToolFunctionInputSchemaProperty
{
	/// <summary>
	/// Gets or sets the type of the property.
	/// </summary>
	[JsonPropertyName("type")]
	public required McpType Type { get; set; }

	/// <summary>
	/// The description of the property, providing additional context or information about its purpose.
	/// </summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}