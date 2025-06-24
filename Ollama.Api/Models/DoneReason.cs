using System.Runtime.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Indicates the reason why the model stopped generating a response.
/// </summary>
public enum DoneReason
{
	/// <summary>
	/// The model stopped naturally, e.g., completed its thought, generated an end-of-sequence token, or hit the num_predict limit.
	/// </summary>
	[EnumMember(Value = "stop")]
	Stop,

	/// <summary>
	/// The model stopped to allow tool calls to be executed (tool_calls array present).
	/// </summary>
	[EnumMember(Value = "tool_calls")]
	ToolCalls,

	/// <summary>
	/// The model hit the maximum context length or token generation limit before completing its thought.
	/// </summary>
	[EnumMember(Value = "length")]
	Length,

	/// <summary>
	/// The model was loaded into memory and reported as done (related to loading state, not response generation).
	/// </summary>
	[EnumMember(Value = "load")]
	Load,

	/// <summary>
	/// The model stopped because the maximum tokens limit was reached (less common, sometimes covered by 'length').
	/// </summary>
	[EnumMember(Value = "max_tokens")]
	MaxTokens,

	/// <summary>
	/// The model stopped because the maximum length limit was reached (less common, sometimes covered by 'length').
	/// </summary>
	[EnumMember(Value = "max_length")]
	MaxLength
}