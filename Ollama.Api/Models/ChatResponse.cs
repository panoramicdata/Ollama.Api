using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response from the chat API.
/// </summary>
public class ChatResponse
{
	/// <summary>
	/// The name of the model used for the chat completion.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// The creation timestamp of the response.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTimeOffset? CreatedAt { get; set; }

	/// <summary>
	/// The message object containing the model's response.
	/// </summary>
	[JsonPropertyName("message")]
	public ChatMessage? Message { get; set; }

	/// <summary>
	/// If the chat completion is not done, this field contains the reason for completion.
	/// </summary>
	[JsonPropertyName("done_reason")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public DoneReason? DoneReason { get; set; }

	/// <summary>
	/// Indicates if the chat completion is complete.
	/// </summary>
	[JsonPropertyName("done")]
	public bool Done { get; set; }

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
