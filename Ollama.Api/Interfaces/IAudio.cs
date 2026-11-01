using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Speech: text to audio, and audio to text, through Ollama's OpenAI-compatible audio endpoints.
/// </summary>
public interface IAudio
{
	/// <summary>
	/// Reads text aloud with a speech model, returning the audio as it is generated.
	/// </summary>
	/// <param name="request">The speech model, the text and the voice.</param>
	/// <param name="cancellationToken">Cancels the call.</param>
	/// <returns>The audio, which must be disposed.</returns>
	/// <exception cref="ApiException">
	/// The model cannot speak, the voice or format is not supported, or the text is too long.
	/// </exception>
	Task<SpeechAudio> CreateSpeechAsync(SpeechRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Transcribes speech into text in the language it was spoken.
	/// </summary>
	/// <exception cref="ApiException">The model does not accept audio, or the audio could not be read.</exception>
	Task<TranscriptionResponse> TranscribeAsync(TranscriptionRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Transcribes speech, yielding each piece of text as it is transcribed, then the whole.
	/// </summary>
	/// <exception cref="ApiException">The model does not accept audio, or the audio could not be read.</exception>
	IAsyncEnumerable<TranscriptionStreamEvent> TranscribeStreamAsync(
		TranscriptionRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Translates speech into text in another language, English unless the request names one.
	/// </summary>
	/// <exception cref="ApiException">The model does not accept audio, or the audio could not be read.</exception>
	Task<TranscriptionResponse> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken);
}
