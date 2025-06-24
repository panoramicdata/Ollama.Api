using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents annotations for a chat tool function input schema, such as title and hints.
/// </summary>
public class ChatToolFunctionInputSchemaAnnotations
{
	/// <summary>
	/// Gets or sets the title of the input schema.
	/// </summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the field is read-only.
	/// </summary>
	[JsonPropertyName("readOnlyHint")]
	public bool? ReadOnlyHint { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the field is open world.
	/// </summary>
	[JsonPropertyName("openWorldHint")]
	public bool? OpenWorldHint { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the operation is destructive.
	/// </summary>
	[JsonPropertyName("destructiveHint")]
	public bool? DestructiveHint { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the operation is idempotent.
	/// </summary>
	[JsonPropertyName("idempotentHint")]
	public bool? IdempotentHint { get; set; }
}