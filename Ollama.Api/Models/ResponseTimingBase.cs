using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Base class for responses that include timing and evaluation metrics.
/// </summary>
public abstract class ResponseTimingBase
{
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
