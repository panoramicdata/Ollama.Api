using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to the chat API for conversational interactions.
/// </summary>
public class ChatRequest : InferenceRequest
{
	/// <summary>
	/// An array of message objects representing the conversation history.
	/// </summary>
	[JsonPropertyName("messages")]
	public required List<ChatMessage> Messages { get; set; }

	/// <summary>
	/// If true, the response will be streamed as a series of newline-delimited JSON objects. If false, the response will be a single JSON object returned once the generation is complete. Defaults to true.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool? Stream { get; set; }

	/// <summary>
	/// A list of tool definitions that the model can use.
	/// </summary>
	[JsonPropertyName("tools")]
	public List<ChatTool>? Tools { get; set; }
}
