using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents detailed information about a model.
/// </summary>
public class ModelDetails
{
	/// <summary>
	/// The parent model this model was derived from.
	/// </summary>
	[JsonPropertyName("parent_model")]
	public string ParentModel { get; set; } = string.Empty;

	/// <summary>
	/// The format of the model (e.g., GGUF, GGML).
	/// </summary>
	[JsonPropertyName("format")]
	public string Format { get; set; } = string.Empty;

	/// <summary>
	/// The family of the model (e.g., llama, mistral).
	/// </summary>
	[JsonPropertyName("family")]
	public string Family { get; set; } = string.Empty;

	/// <summary>
	/// The families this model belongs to.
	/// </summary>
	[JsonPropertyName("families")]
	public string[] Families { get; set; } = [];

	/// <summary>
	/// The parameter size of the model (e.g., 7B, 13B).
	/// </summary>
	[JsonPropertyName("parameter_size")]
	public string ParameterSize { get; set; } = string.Empty;

	/// <summary>
	/// The quantization level of the model (e.g., Q4_0, Q5_1).
	/// </summary>
	[JsonPropertyName("quantization_level")]
	public string QuantizationLevel { get; set; } = string.Empty;

	/// <summary>
	/// The engine that runs the model, such as <c>llamacpp</c> or <c>mlx</c>.
	/// </summary>
	[JsonPropertyName("runner")]
	public string? Runner { get; set; }
}
