using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ollama.Api;
/// <summary>
/// The settings an <see cref="OllamaClient"/> is built from.
/// </summary>
public class OllamaClientOptions
{
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
}
