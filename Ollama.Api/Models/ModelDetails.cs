using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

public class ModelDetails
{
	/// <summary>
	/// The parent model this model was derived from.
	/// </summary>
	[JsonPropertyName("parent_model")]
	public required string ParentModel { get; set; }

	/// <summary>
	/// The format of the model (e.g., GGUF, GGML).
	/// </summary>
	[JsonPropertyName("format")]
	public required string Format { get; set; }

	/// <summary>
	/// The family of the model (e.g., llama, mistral).
	/// </summary>
	[JsonPropertyName("family")]
	public required string Family { get; set; }

	/// <summary>
	/// The families this model belongs to.
	/// </summary>
	[JsonPropertyName("families")]
	public required string[] Families { get; set; }

	/// <summary>
	/// The parameter size of the model (e.g., 7B, 13B).
	/// </summary>
	[JsonPropertyName("parameter_size")]
	public required string ParameterSize { get; set; }

	/// <summary>
	/// The quantization level of the model (e.g., Q4_0, Q5_1).
	/// </summary>
	[JsonPropertyName("quantization_level")]
	public required string QuantizationLevel { get; set; }
}
