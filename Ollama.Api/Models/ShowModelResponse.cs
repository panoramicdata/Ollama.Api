using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response containing detailed information about a specific local model.
/// </summary>
public class ShowModelResponse
{
	/// <summary>
	/// The last modified timestamp of the model.
	/// </summary>
	[JsonPropertyName("modified_at")]
	public DateTimeOffset ModifiedAt { get; set; }

	/// <summary>
	/// The model's format, family, size and quantisation.
	/// </summary>
	[JsonPropertyName("details")]
	public ModelDetails Details { get; set; } = new();

	/// <summary>
	/// What the model can do, such as <c>completion</c>, <c>tools</c>, <c>thinking</c>, <c>vision</c>,
	/// <c>embedding</c>, <c>speech</c> or <c>decision</c>.
	/// </summary>
	[JsonPropertyName("capabilities")]
	public List<string>? Capabilities { get; set; }

	/// <summary>
	/// The thinking values the model accepts and its default, or null for a model with no thinking metadata.
	/// </summary>
	[JsonPropertyName("thinking")]
	public ThinkingInfo? Thinking { get; set; }

	/// <summary>
	/// The voices a speech model can speak with.
	/// </summary>
	[JsonPropertyName("voices")]
	public List<string>? Voices { get; set; }

	/// <summary>
	/// The model's licence text.
	/// </summary>
	[JsonPropertyName("license")]
	public string? License { get; set; }

	/// <summary>
	/// A Modelfile that would recreate the model.
	/// </summary>
	[JsonPropertyName("modelfile")]
	public string? Modelfile { get; set; }

	/// <summary>
	/// The model's parameter settings, one per line.
	/// </summary>
	[JsonPropertyName("parameters")]
	public string? Parameters { get; set; }

	/// <summary>
	/// The prompt template.
	/// </summary>
	[JsonPropertyName("template")]
	public string? Template { get; set; }

	/// <summary>
	/// The chat renderer the model uses, when one is set.
	/// </summary>
	[JsonPropertyName("renderer")]
	public string? Renderer { get; set; }

	/// <summary>
	/// The output parser the model uses, when one is set.
	/// </summary>
	[JsonPropertyName("parser")]
	public string? Parser { get; set; }

	/// <summary>
	/// Where the chat prompt comes from, such as <c>renderer_parser</c>, <c>go_template</c> or
	/// <c>gguf_chat_template</c>.
	/// </summary>
	[JsonPropertyName("template_source")]
	public string? TemplateSource { get; set; }

	/// <summary>
	/// The lowest Ollama version that can run the model.
	/// </summary>
	[JsonPropertyName("requires")]
	public string? Requires { get; set; }

	/// <summary>
	/// The model's metadata, keyed by GGUF field name.
	/// </summary>
	[JsonPropertyName("model_info")]
	public Dictionary<string, object?>? ModelInfo { get; set; }

	/// <summary>
	/// The vision or audio projector's metadata, for a multimodal model.
	/// </summary>
	[JsonPropertyName("projector_info")]
	public Dictionary<string, object?>? ProjectorInfo { get; set; }
}
