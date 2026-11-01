using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The text a transcription or translation produced.
/// </summary>
public class TranscriptionResponse
{
	/// <summary>
	/// The transcribed or translated text.
	/// </summary>
	[JsonPropertyName("text")]
	public string Text { get; set; } = string.Empty;

	/// <summary>
	/// <c>transcribe</c> or <c>translate</c>, for <see cref="TranscriptionFormat.VerboseJson"/>.
	/// </summary>
	[JsonPropertyName("task")]
	public string? Task { get; set; }

	/// <summary>
	/// The language, for <see cref="TranscriptionFormat.VerboseJson"/>: the output language of a
	/// translation, or the requested language of a transcription.
	/// </summary>
	[JsonPropertyName("language")]
	public string? Language { get; set; }

	/// <summary>
	/// The audio's length in seconds, for <see cref="TranscriptionFormat.VerboseJson"/>.
	/// </summary>
	[JsonPropertyName("duration")]
	public double? Duration { get; set; }

	/// <summary>
	/// The text in timed pieces, for <see cref="TranscriptionFormat.VerboseJson"/>.
	/// </summary>
	[JsonPropertyName("segments")]
	public List<TranscriptionSegment>? Segments { get; set; }
}

/// <summary>
/// One timed piece of a transcription.
/// </summary>
public class TranscriptionSegment
{
	/// <summary>
	/// The segment's position, from 0.
	/// </summary>
	[JsonPropertyName("id")]
	public int Id { get; set; }

	/// <summary>
	/// Where the segment starts, in seconds.
	/// </summary>
	[JsonPropertyName("start")]
	public double Start { get; set; }

	/// <summary>
	/// Where the segment ends, in seconds.
	/// </summary>
	[JsonPropertyName("end")]
	public double End { get; set; }

	/// <summary>
	/// The segment's text.
	/// </summary>
	[JsonPropertyName("text")]
	public string Text { get; set; } = string.Empty;
}

/// <summary>
/// One server-sent event from a streamed transcription or translation.
/// </summary>
public class TranscriptionStreamEvent
{
	/// <summary>
	/// <c>transcript.text.delta</c> for a piece of text, or <c>transcript.text.done</c> for the whole.
	/// </summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>
	/// The newly transcribed text, on a delta event.
	/// </summary>
	[JsonPropertyName("delta")]
	public string? Delta { get; set; }

	/// <summary>
	/// The complete text, on the done event.
	/// </summary>
	[JsonPropertyName("text")]
	public string? Text { get; set; }

	/// <summary>
	/// True on the event that ends the stream.
	/// </summary>
	[JsonIgnore]
	public bool IsDone => Type == "transcript.text.done";
}
