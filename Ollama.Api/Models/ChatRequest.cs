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
	/// Whether a model that can reason does so before it answers: true, false, or a level the model
	/// names, such as "high". Null leaves the model's default.
	/// </summary>
	[JsonPropertyName("think")]
	public ThinkValue? Think { get; set; }

	/// <summary>
	/// Whether to return the log probability of each generated token.
	/// </summary>
	[JsonPropertyName("logprobs")]
	public bool? Logprobs { get; set; }

	/// <summary>
	/// How many of the likeliest alternatives to return at each position, when <see cref="Logprobs"/> is set.
	/// </summary>
	[JsonPropertyName("top_logprobs")]
	public int? TopLogprobs { get; set; }

	/// <summary>
	/// The output format: the string "json", or a JSON schema object the reply must match.
	/// </summary>
	[JsonPropertyName("format")]
	public object? Format { get; set; }

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
