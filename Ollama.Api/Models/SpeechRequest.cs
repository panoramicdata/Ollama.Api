using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A request to <c>/v1/audio/speech</c> to read text aloud with a speech model.
/// </summary>
public class SpeechRequest
{
	/// <summary>
	/// A speech model, such as <c>lfm3-audio</c> or <c>phonon</c>.
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// The text to speak: up to 4,096 characters.
	/// </summary>
	[JsonPropertyName("input")]
	public required string Input { get; set; }

	/// <summary>
	/// One of the voices <c>/api/show</c> lists for the model, in any case. Null for the model's default voice.
	/// </summary>
	[JsonPropertyName("voice")]
	public string? Voice { get; set; }

	/// <summary>
	/// The audio format. Null for the server default, WAV.
	/// </summary>
	[JsonPropertyName("response_format")]
	public SpeechFormat? ResponseFormat { get; set; }

	/// <summary>
	/// The playback speed. Ollama accepts only 1.
	/// </summary>
	[JsonPropertyName("speed")]
	public double? Speed { get; set; }
}

/// <summary>
/// The audio formats a speech model returns: 16-bit mono samples at 24 kHz.
/// </summary>
public enum SpeechFormat
{
	/// <summary>
	/// A WAV file. Its size fields hold the maximum value, because the header is written before the
	/// length is known.
	/// </summary>
	Wav,

	/// <summary>
	/// Raw little-endian samples with no header.
	/// </summary>
	Pcm
}
