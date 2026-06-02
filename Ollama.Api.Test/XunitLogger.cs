using Microsoft.Extensions.Logging;

namespace Ollama.Api.Test;

/// <summary>
/// Logger that writes to XUnit's ITestOutputHelper
/// </summary>
public class XunitLogger(ITestOutputHelper testOutputHelper, string categoryName) : ILogger
{
	private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
	private readonly string _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	public bool IsEnabled(LogLevel logLevel)
		=> logLevel >= LogLevel.Debug;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		var message = formatter(state, exception);
		var logLine = $"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";

		if (exception != null)
		{
			logLine += Environment.NewLine + exception;
		}

		try
		{
			_testOutputHelper.WriteLine(logLine);
		}
		catch
		{
			// Ignore if test output is no longer available
		}
	}
}
