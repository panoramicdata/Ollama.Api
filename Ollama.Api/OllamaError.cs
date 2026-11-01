using System.Text.Json;

namespace Ollama.Api;

/// <summary>
/// Reads the message out of an Ollama error body.
/// </summary>
internal static class OllamaError
{
	/// <summary>
	/// The error message in <paramref name="body"/>, or the body itself when it holds no recognisable
	/// message, or null when there is no body.
	/// </summary>
	/// <remarks>
	/// The native endpoints answer <c>{"error": "..."}</c>; the OpenAI-compatible <c>/v1</c> ones answer
	/// <c>{"error": {"message": "..."}}</c>.
	/// </remarks>
	internal static string? Extract(string? body)
	{
		if (string.IsNullOrWhiteSpace(body))
		{
			return null;
		}

		try
		{
			using var document = JsonDocument.Parse(body);

			return FindMessage(document.RootElement) ?? body;
		}
		catch (JsonException)
		{
			return body;
		}
	}

	private static string? FindMessage(JsonElement root)
	{
		if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("error", out var error))
		{
			return null;
		}

		if (error.ValueKind == JsonValueKind.String)
		{
			return error.GetString();
		}

		return error.ValueKind == JsonValueKind.Object
			&& error.TryGetProperty("message", out var message)
			&& message.ValueKind == JsonValueKind.String
				? message.GetString()
				: null;
	}
}
