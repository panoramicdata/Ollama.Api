using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response from the chat API.
/// </summary>
public class ChatResponse : ResponseTimingBase
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
}
