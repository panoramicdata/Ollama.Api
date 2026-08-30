using Microsoft.Extensions.Logging;

namespace Ollama.Api.Test;

/// <summary>
/// Logger provider that writes to XUnit's ITestOutputHelper
/// </summary>
public class XunitLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
	private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

	/// <summary>
	/// Creates a logger that writes to the test output under the supplied category.
	/// </summary>
	/// <param name="categoryName">The category to tag each line with.</param>
	/// <returns>The logger.</returns>
	public ILogger CreateLogger(string categoryName)
		=> new XunitLogger(_testOutputHelper, categoryName);

	/// <inheritdoc />
	public void Dispose()
		=> GC.SuppressFinalize(this);
}
