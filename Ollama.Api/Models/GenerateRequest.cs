using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to generate text using a specified model and prompt.
/// </summary>
public class GenerateRequest
{
	/// <summary>
	/// The name of the model to use for generating the completion.
	/// The model must be available on the server.
	/// </summary>
	/// <example>llama3.1</example>
	/// <example>mistral</example>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// The initial text input that serves as the basis for the generated completion.
	/// The quality and relevance of the generated text depend on the clarity and context provided in this prompt.
	/// </summary>
	[JsonPropertyName("prompt")]
	public required string Prompt { get; set; }

	/// <summary>
	/// If true, the response will be streamed as a series of newline-delimited JSON objects.
	/// If false, the response will be a single JSON object returned once the generation is complete.
	/// Defaults to true.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool Stream { get; set; }

	/// <summary>
	/// An object containing advanced model parameters to fine-tune the generation process.
	/// These options override any defaults defined in the model's Modelfile.
	/// See "Model Options" section for details.
	/// </summary>
	[JsonPropertyName("options")]
	public GenerateOptions? Options { get; set; }

	/// <summary>
	/// Specifies the desired output format.
	/// Currently, the only supported value is "json".
	/// If set, Ollama will attempt to format the model's response as a valid JSON object.
	/// The model itself must be instructed to produce JSON for this to be effective.
	/// </summary>
	[JsonPropertyName("format")]
	public string? Format { get; set; }

	/// <summary>
	/// If true, the prompt is sent directly to the model without any built-in templating.
	/// Use this if you are providing a full templated prompt that includes instructions, system messages, and chat history yourself.
	/// Defaults to false.
	/// </summary>
	[JsonPropertyName("raw")]
	public bool? Raw { get; set; }

	/// <summary>
	/// Text to append after the model's generated response.
	/// This can be useful for controlling the structure of the output or signaling the end of a thought.
	/// </summary>
	[JsonPropertyName("suffix")]
	public string? Suffix { get; set; }

	/// <summary>
	/// A list of base64-encoded images to be included with the prompt.
	/// Only applicable for multimodal models (e.g., llava).
	/// </summary>
	[JsonPropertyName("images")]
	public List<string>? Images { get; set; }

	/// <summary>
	/// A system message that overrides any default system prompt defined in the model's Modelfile.
	/// Used to set the persona or instructions for the model.
	/// </summary>
	[JsonPropertyName("system")]
	public string? System { get; set; }

	/// <summary>
	/// A custom prompt template that overrides any default template defined in the model's Modelfile.
	/// </summary>
	[JsonPropertyName("template")]
	public string? Template { get; set; }

	/// <summary>
	/// The context array returned from a previous /api/generate request. Used to maintain conversational memory.
	/// </summary>
	[JsonPropertyName("context")]
	public List<int>? Context { get; set; }

	/// <summary>
	/// Controls how long the model will stay loaded into memory following the request.
	/// Can be a duration string (e.g., "5m", "10s") or an integer representing seconds. Defaults to "5m".
	/// </summary>
	[JsonPropertyName("keep_alive")]
	public string? KeepAlive { get; set; }
}
