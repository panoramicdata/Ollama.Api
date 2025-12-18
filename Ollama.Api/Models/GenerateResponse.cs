using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response from the generate API.
/// </summary>
public class GenerateResponse : ResponseTimingBase
{
	/// <summary>
	/// The name of the model used for generation.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// The creation timestamp of the response.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTimeOffset? CreatedAt { get; set; }

	/// <summary>
	/// The generated response text.
	/// </summary>
	[JsonPropertyName("response")]
	public string? Response { get; set; }

	/// <summary>
	/// Indicates if the generation is complete.
	/// </summary>
	[JsonPropertyName("done")]
	public bool Done { get; set; }

	/// <summary>
	/// The context window as a list of token IDs.
	/// </summary>
	[JsonPropertyName("context")]
	public List<int>? Context { get; set; }
}
