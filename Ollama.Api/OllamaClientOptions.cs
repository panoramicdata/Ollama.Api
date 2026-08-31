using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ollama.Api;
/// <summary>
/// The settings an <see cref="OllamaClient"/> is built from.
/// </summary>
public class OllamaClientOptions
{
	/// <summary>How long an Ollama call may take when no timeout is configured.</summary>
	/// <remarks>
	/// Some calls — model retrieval especially — legitimately take many minutes, so the default is
	/// generous rather than typical.
	/// </remarks>
	public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(30);

	/// <summary>
	/// The base address of the Ollama server, for example <c>http://localhost:11434</c>.
	/// </summary>
	public required Uri Uri { get; init; }

	/// <summary>
	/// Where the client writes its request and response logging. Defaults to logging nothing.
	/// </summary>
	/// <remarks>
	/// Enabling <c>Debug</c> here logs every request and every buffered response body in full.
	/// Streamed responses are deliberately left unread, so turning this on does not break streaming.
	/// </remarks>
	public ILogger Logger { get; init; } = NullLogger.Instance;

	/// <summary>
	/// An optional API key, sent as a bearer token on every request.
	/// </summary>
	/// <remarks>
	/// Null or blank for a local Ollama, which needs no authentication and is the common case. Set it
	/// for a hosted instance or a cloud service that expects one.
	/// </remarks>
	public string? ApiKey { get; init; }

	/// <summary>
	/// How long a call may take, or null for <see cref="DefaultTimeout"/>.
	/// </summary>
	/// <remarks>
	/// Worth setting where a caller would rather fail than wait: a long agentic loop against a small
	/// model is more useful with a bounded per-call wait than with a thirty-minute one.
	/// </remarks>
	public TimeSpan? Timeout { get; init; }

	/// <summary>
	/// The timeout actually applied: <see cref="Timeout"/> where given, otherwise
	/// <see cref="DefaultTimeout"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// The configured timeout is zero or negative, which would fail every call the instant it was made
	/// — indistinguishable, to whoever is reading the error, from the server being down.
	/// </exception>
	public TimeSpan EffectiveTimeout => Timeout switch
	{
		null => DefaultTimeout,
		var t when t <= TimeSpan.Zero => throw new InvalidOperationException(
			$"{nameof(Timeout)} must be positive; {t} would fail every call immediately."),
		var t => t.Value
	};
}
