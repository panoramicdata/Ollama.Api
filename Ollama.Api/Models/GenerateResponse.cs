using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response from the generate API.
/// </summary>
public class GenerateResponse
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

	/// <summary>
	/// Total duration of the request in nanoseconds.
	/// </summary>
	[JsonPropertyName("total_duration")]
	public long? TotalDuration { get; set; }

	/// <summary>
	/// Duration to load the model in nanoseconds.
	/// </summary>
	[JsonPropertyName("load_duration")]
	public long? LoadDuration { get; set; }

	/// <summary>
	/// Number of prompt tokens evaluated.
	/// </summary>
	[JsonPropertyName("prompt_eval_count")]
	public int? PromptEvalCount { get; set; }

	/// <summary>
	/// Duration to evaluate the prompt in nanoseconds.
	/// </summary>
	[JsonPropertyName("prompt_eval_duration")]
	public long? PromptEvalDuration { get; set; }

	/// <summary>
	/// Number of tokens evaluated during generation.
	/// </summary>
	[JsonPropertyName("eval_count")]
	public int? EvalCount { get; set; }

	/// <summary>
	/// Duration to evaluate tokens during generation in nanoseconds.
	/// </summary>
	[JsonPropertyName("eval_duration")]
	public long? EvalDuration { get; set; }
}
