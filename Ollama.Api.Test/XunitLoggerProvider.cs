using Microsoft.Extensions.Logging;

namespace Ollama.Api.Test;

/// <summary>
/// Logger provider that writes to XUnit's ITestOutputHelper
/// </summary>
public class XunitLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
	private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

	public ILogger CreateLogger(string categoryName)
		=> new XunitLogger(_testOutputHelper, categoryName);

	public void Dispose()
		=> GC.SuppressFinalize(this);
}
