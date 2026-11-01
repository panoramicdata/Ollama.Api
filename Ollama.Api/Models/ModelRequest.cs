using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A request about one model, named in <see cref="Model"/>.
/// </summary>
public abstract class ModelRequest
{
	/// <summary>
	/// The model's full name, including its tag (e.g., "llama3.1:latest").
	/// </summary>
	[JsonPropertyName("model")]
	public string Model { get; set; } = string.Empty;

	/// <summary>
	/// The model's name. Ollama now calls this field <c>model</c>.
	/// </summary>
	[JsonIgnore]
	[Obsolete("Use Model, which is the name Ollama gives this field.")]
	public string Name
	{
		get => Model;
		set => Model = value;
	}
}
