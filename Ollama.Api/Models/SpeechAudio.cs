namespace Ollama.Api.Models;

/// <summary>
/// Speech audio as it streams from the server. Dispose it to release the connection.
/// </summary>
public sealed class SpeechAudio : IDisposable, IAsyncDisposable
{
	private readonly HttpResponseMessage _response;

	internal SpeechAudio(HttpResponseMessage response, Stream content)
	{
		_response = response;
		Content = content;
	}

	/// <summary>
	/// The audio, readable as it is generated, so playback can start before the speech is complete.
	/// </summary>
	public Stream Content { get; }

	/// <summary>
	/// The audio's media type, such as <c>audio/wav</c> or <c>audio/pcm</c>.
	/// </summary>
	public string? ContentType => _response.Content.Headers.ContentType?.MediaType;

	/// <summary>
	/// Copies all of the audio to <paramref name="destination"/>, such as a file.
	/// </summary>
	public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
		=> Content.CopyToAsync(destination, cancellationToken);

	/// <inheritdoc />
	public void Dispose()
	{
		Content.Dispose();
		_response.Dispose();
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await Content.DisposeAsync().ConfigureAwait(false);
		_response.Dispose();
	}
}
