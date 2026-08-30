using Microsoft.Extensions.Logging;

namespace Ollama.Api.Test;

/// <summary>
/// Logger that writes to XUnit's ITestOutputHelper
/// </summary>
public class XunitLogger(ITestOutputHelper testOutputHelper, string categoryName) : ILogger
{
	private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
	private readonly string _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));

	/// <summary>
	/// Scopes are not supported; always returns null.
	/// </summary>
	/// <typeparam name="TState">The scope state type.</typeparam>
	/// <param name="state">The scope state, ignored.</param>
	/// <returns>Null.</returns>
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	/// <summary>
	/// True for Debug and above, so that the client's request and response logging reaches the test
	/// output.
	/// </summary>
	/// <param name="logLevel">The level being tested.</param>
	/// <returns>Whether messages at that level are written.</returns>
	public bool IsEnabled(LogLevel logLevel)
		=> logLevel >= LogLevel.Debug;

	/// <summary>
	/// Writes one formatted line to the test output, ignoring the write if the test has already
	/// finished and its output is no longer available.
	/// </summary>
	/// <typeparam name="TState">The state type.</typeparam>
	/// <param name="logLevel">The level of the message.</param>
	/// <param name="eventId">The event id, unused.</param>
	/// <param name="state">The state to format.</param>
	/// <param name="exception">The exception to append, if any.</param>
	/// <param name="formatter">Turns the state and exception into the message.</param>
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
