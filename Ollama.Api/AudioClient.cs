using Ollama.Api.Interfaces;
using Ollama.Api.Models;
using Refit;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ollama.Api;

/// <summary>
/// The audio endpoints, sent by hand rather than through Refit: speech is a binary stream that must
/// reach the caller unbuffered, and transcription is a multipart upload.
/// </summary>
internal sealed class AudioClient(HttpClient httpClient, RefitSettings refitSettings) : IAudio
{
	private const string DonePayload = "[DONE]";

	public async Task<SpeechAudio> CreateSpeechAsync(SpeechRequest request, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);

		using var message = new HttpRequestMessage(HttpMethod.Post, "/v1/audio/speech")
		{
			Content = JsonContent.Create(request, options: OllamaClient.JsonSerializerOptions)
		};

		var response = await SendAsync(message, cancellationToken).ConfigureAwait(false);
		var audio = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

		return new SpeechAudio(response, audio);
	}

	public Task<TranscriptionResponse> TranscribeAsync(TranscriptionRequest request, CancellationToken cancellationToken)
		=> PostTranscriptionAsync("/v1/audio/transcriptions", BuildForm(request), cancellationToken);

	public Task<TranscriptionResponse> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken)
	{
		var form = BuildForm(request);
		AddIfSet(form, "to_language", request.ToLanguage);

		return PostTranscriptionAsync("/v1/audio/translations", form, cancellationToken);
	}

	public async IAsyncEnumerable<TranscriptionStreamEvent> TranscribeStreamAsync(
		TranscriptionRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var form = BuildForm(request);
		form.Add(new StringContent("true"), "stream");

		using var message = new HttpRequestMessage(HttpMethod.Post, "/v1/audio/transcriptions") { Content = form };
		using var response = await SendAsync(message, cancellationToken).ConfigureAwait(false);
		using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
		{
			var streamEvent = ParseEvent(line);

			if (streamEvent is null)
			{
				continue;
			}

			yield return streamEvent;

			if (streamEvent.IsDone)
			{
				yield break;
			}
		}
	}

	/// <summary>
	/// The event on one line of a server-sent event stream, or null for a line that carries none.
	/// </summary>
	internal static TranscriptionStreamEvent? ParseEvent(string line)
	{
		if (!line.StartsWith("data:", StringComparison.Ordinal))
		{
			return null;
		}

		var payload = line[5..].Trim();

		return payload.Length == 0 || payload == DonePayload
			? null
			: JsonSerializer.Deserialize<TranscriptionStreamEvent>(payload, OllamaClient.JsonSerializerOptions);
	}

	private async Task<TranscriptionResponse> PostTranscriptionAsync(
		string path,
		MultipartFormDataContent form,
		CancellationToken cancellationToken)
	{
		using var message = new HttpRequestMessage(HttpMethod.Post, path) { Content = form };
		using var response = await SendAsync(message, cancellationToken).ConfigureAwait(false);
		var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		// The text format is the transcript alone, as text/plain.
		return response.Content.Headers.ContentType?.MediaType?.EndsWith("json", StringComparison.OrdinalIgnoreCase) == true
			? JsonSerializer.Deserialize<TranscriptionResponse>(body, OllamaClient.JsonSerializerOptions)!
			: new TranscriptionResponse { Text = body.TrimEnd('\r', '\n') };
	}

	/// <summary>
	/// Sends the request, returning the response once its headers arrive, or throwing for a failure.
	/// </summary>
	private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
	{
		var response = await httpClient
			.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
			.ConfigureAwait(false);

		if (response.IsSuccessStatusCode)
		{
			return response;
		}

		using (response)
		{
			throw await ApiException
				.Create(message, message.Method, response, refitSettings)
				.ConfigureAwait(false);
		}
	}

	internal static MultipartFormDataContent BuildForm(TranscriptionRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		var audio = new LeaveOpenStreamContent(request.Audio);
		audio.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

		var form = new MultipartFormDataContent
		{
			{ audio, "file", request.FileName },
			{ new StringContent(request.Model), "model" }
		};

		AddIfSet(form, "language", request.Language);
		AddIfSet(form, "prompt", request.Prompt);
		AddIfSet(form, "temperature", request.Temperature?.ToString(CultureInfo.InvariantCulture));
		AddIfSet(form, "response_format", ToWireName(request.ResponseFormat));

		return form;
	}

	internal static string? ToWireName(TranscriptionFormat? format) => format switch
	{
		TranscriptionFormat.Json => "json",
		TranscriptionFormat.VerboseJson => "verbose_json",
		TranscriptionFormat.Text => "text",
		_ => null
	};

	private static void AddIfSet(MultipartFormDataContent form, string name, string? value)
	{
		if (value is not null)
		{
			form.Add(new StringContent(value), name);
		}
	}
}
