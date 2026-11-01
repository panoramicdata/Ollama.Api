using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to create a new custom model in Ollama.
/// </summary>
public class CreateModelRequest : ModelRequest
{
	/// <summary>
	/// The existing model to create from (e.g., "llama3.1:latest"). Not needed when <see cref="Files"/> is given.
	/// </summary>
	[JsonPropertyName("from")]
	public string? From { get; set; }

	/// <summary>
	/// A system message to set the persona or instructions for the model (optional).
	/// </summary>
	[JsonPropertyName("system")]
	public string? System { get; set; }

	/// <summary>
	/// The prompt template.
	/// </summary>
	[JsonPropertyName("template")]
	public string? Template { get; set; }

	/// <summary>
	/// The chat renderer to use.
	/// </summary>
	[JsonPropertyName("renderer")]
	public string? Renderer { get; set; }

	/// <summary>
	/// The output parser to use.
	/// </summary>
	[JsonPropertyName("parser")]
	public string? Parser { get; set; }

	/// <summary>
	/// Source file names mapped to the SHA-256 digests of blobs already pushed with
	/// <see cref="Interfaces.IModels.PushBlobAsync"/>. A split GGUF lists each shard under its own name.
	/// </summary>
	[JsonPropertyName("files")]
	public Dictionary<string, string>? Files { get; set; }

	/// <summary>
	/// Draft model file names mapped to their SHA-256 digests, for speculative decoding.
	/// </summary>
	[JsonPropertyName("draft_files")]
	public Dictionary<string, string>? DraftFiles { get; set; }

	/// <summary>
	/// The licence or licences the model is released under.
	/// </summary>
	[JsonPropertyName("license")]
	public List<string>? License { get; set; }

	/// <summary>
	/// Model parameters, such as <c>temperature</c> or <c>num_ctx</c>.
	/// </summary>
	[JsonPropertyName("parameters")]
	public Dictionary<string, object>? Parameters { get; set; }

	/// <summary>
	/// A conversation to start every chat with.
	/// </summary>
	[JsonPropertyName("messages")]
	public List<ChatMessage>? Messages { get; set; }

	/// <summary>
	/// The quantisation to apply on import, such as <c>q4_K_M</c> or <c>nvfp4</c>.
	/// </summary>
	[JsonPropertyName("quantize")]
	public string? Quantize { get; set; }

	/// <summary>
	/// The quantisation to apply to the draft model's weights on import.
	/// </summary>
	[JsonPropertyName("draft_quantize")]
	public string? DraftQuantize { get; set; }

	/// <summary>
	/// The lowest Ollama version that can run the model.
	/// </summary>
	[JsonPropertyName("requires")]
	public string? Requires { get; set; }

	/// <summary>
	/// Capabilities to declare in addition to those inherited or inferred, such as <c>decision</c>.
	/// </summary>
	[JsonPropertyName("capabilities")]
	public List<string>? Capabilities { get; set; }

	/// <summary>
	/// Whether to stream progress updates. Defaults to true on the server.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool? Stream { get; set; }

	/// <summary>
	/// The path to a Modelfile on the server. Superseded by the fields above.
	/// </summary>
	[JsonPropertyName("path")]
	public string? Path { get; set; }
}
