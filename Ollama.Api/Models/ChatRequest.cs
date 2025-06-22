using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to the chat API for conversational interactions.
/// </summary>
public class ChatRequest
{
	/// <summary>
	/// The name of the model to use for generating the chat completion.
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

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
	/// An object containing advanced model parameters to fine-tune the generation process.
	/// </summary>
	[JsonPropertyName("options")]
	public GenerateOptions? Options { get; set; }

	/// <summary>
	/// Specifies the desired output format. Currently, the only supported value is "json".
	/// </summary>
	[JsonPropertyName("format")]
	public string? Format { get; set; }

	/// <summary>
	/// A list of tool definitions that the model can use.
	/// </summary>
	[JsonPropertyName("tools")]
	public List<ChatTool>? Tools { get; set; }

	/// <summary>
	/// Controls how long the model will stay loaded into memory following the request.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	public string? KeepAlive { get; set; }
}
