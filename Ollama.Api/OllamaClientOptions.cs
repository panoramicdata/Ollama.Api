using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ollama.Api;
public class OllamaClientOptions
{
	public required Uri Uri { get; init; }
	public ILogger Logger { get; init; } = NullLogger.Instance;
}
