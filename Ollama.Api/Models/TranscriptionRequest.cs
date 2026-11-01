namespace Ollama.Api.Models;

/// <summary>
/// A request to <c>/v1/audio/transcriptions</c> to turn speech into text.
/// </summary>
public class TranscriptionRequest
{
	/// <summary>
	/// A model that accepts audio, such as <c>lfm3-audio</c>.
	/// </summary>
	public required string Model { get; set; }

	/// <summary>
	/// The audio: a WAV file of up to 512 MB and two hours. The caller keeps ownership of the stream.
	/// </summary>
	public required Stream Audio { get; set; }

	/// <summary>
	/// The file name sent with the audio.
	/// </summary>
	public string FileName { get; set; } = "audio.wav";

	/// <summary>
	/// The spoken language as an ISO 639-1 code, such as <c>fr</c>, or null to let the model detect it.
	/// </summary>
	public string? Language { get; set; }

	/// <summary>
	/// Guidance for a model without speech-specific instructions.
	/// </summary>
	public string? Prompt { get; set; }

	/// <summary>
	/// The sampling temperature. Null for the server default, 0.
	/// </summary>
	public double? Temperature { get; set; }

	/// <summary>
	/// The response format. Null for the server default, <see cref="TranscriptionFormat.Json"/>.
	/// </summary>
	public TranscriptionFormat? ResponseFormat { get; set; }
}

/// <summary>
/// A request to <c>/v1/audio/translations</c> to turn speech into text in another language.
/// </summary>
public class TranslationRequest : TranscriptionRequest
{
	/// <summary>
	/// The output language as an ISO 639-1 code. Null for the server default, English.
	/// </summary>
	public string? ToLanguage { get; set; }
}

/// <summary>
/// The shapes a transcription can be returned in.
/// </summary>
public enum TranscriptionFormat
{
	/// <summary>
	/// The text alone.
	/// </summary>
	Json,

	/// <summary>
	/// The text with its task, language, duration and timed segments.
	/// </summary>
	VerboseJson,

	/// <summary>
	/// Plain text.
	/// </summary>
	Text
}
