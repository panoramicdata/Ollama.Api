using Ollama.Api.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ollama.Api;

/// <summary>
/// Reads Ollama's newline-delimited JSON chat stream (issue #1).
/// </summary>
/// <remarks>
/// <para>
/// Deliberately not routed through Refit. Refit's job is turning a response body into an object, and
/// a stream has no single body to turn into anything - asking it to hand back partially-read content
/// means fighting its content serialiser and its buffering. Posting through the same
/// <see cref="HttpClient"/> and reading the response ourselves is both shorter and clearer about what
/// is happening.
/// </para>
/// <para>
/// <see cref="HttpCompletionOption.ResponseHeadersRead"/> is the load-bearing detail. Without it
/// <see cref="HttpClient"/> buffers the entire response before returning, which produces code that
/// looks like streaming, passes a test that only checks the chunks, and shows the user nothing until
/// the generation has finished - the exact failure this is meant to remove.
/// </para>
/// </remarks>
internal static class ChatStreamReader
{
	/// <summary>
	/// Posts a chat request with streaming enabled and yields each response object as it arrives.
	/// </summary>
	internal static async IAsyncEnumerable<ChatResponse> ReadAsync(
		HttpClient httpClient,
		ChatRequest chatRequest,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
		{
			Content = JsonContent.Create(chatRequest, options: OllamaClient.JsonSerializerOptions)
		};

		using var response = await httpClient
			.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
			.ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			// Reported the same way ChatAsync reports it - as a response carrying an error - so a
			// consumer's enumeration does not have to distinguish a transport fault from a model
			// refusal partway through a loop.
			var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

			yield return new ChatResponse
			{
				Done = true,
				Error = ExtractError(body) ?? response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}"
			};

			yield break;
		}

		using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		using var reader = new StreamReader(stream);

		while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			// Parsed outside the yield, because a yield cannot sit inside a catch block. A malformed
			// line is reported rather than skipped: dropping chunks silently would show the user an
			// answer with holes in it and no indication anything was lost.
			ChatResponse? chunk = null;
			string? parseError = null;

			try
			{
				chunk = JsonSerializer.Deserialize<ChatResponse>(line, OllamaClient.JsonSerializerOptions);
			}
			catch (JsonException exception)
			{
				parseError = exception.Message;
			}

			if (parseError is not null)
			{
				yield return new ChatResponse
				{
					Done = true,
					Error = $"Could not parse a streamed chat response: {parseError}"
				};

				yield break;
			}

			if (chunk is null)
			{
				continue;
			}

			yield return chunk;

			if (chunk.Done)
			{
				yield break;
			}
		}
	}

	/// <summary>
	/// Pulls the message out of Ollama's <c>{"error": "..."}</c> body, or returns null if the body is
	/// not that shape.
	/// </summary>
	private static string? ExtractError(string? body)
	{
		if (string.IsNullOrWhiteSpace(body))
		{
			return null;
		}

		try
		{
			using var document = JsonDocument.Parse(body);

			return document.RootElement.TryGetProperty("error", out var error)
				&& error.ValueKind == JsonValueKind.String
					? error.GetString()
					: body;
		}
		catch (JsonException)
		{
			return body;
		}
	}
}
