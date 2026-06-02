using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a tool call in a chat message.
/// </summary>
public class ChatToolCall
{
	/// <summary>
	/// The unique identifier for the tool call.
	/// This is used to match tool calls with their results in the response.
	/// The model generates this ID when it makes a tool call, and it is included in the response when the tool call is completed.
	/// </summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>
	/// The function call details for the tool call.
	/// </summary>
	[JsonPropertyName("function")]
	public required ChatToolFunctionCall Function { get; set; }
}
