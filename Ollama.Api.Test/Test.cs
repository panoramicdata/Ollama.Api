using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ollama.Api.Test;

/// <summary>
/// The base class for the test classes: builds an <see cref="OllamaClient"/> pointed at the server
/// named in configuration, logging through the test's output.
/// </summary>
public class Test : TestBed<Fixture>, IAsyncDisposable
{
	/// <summary>
	/// The model the tests use unless they name another one.
	/// </summary>
	protected static string TestModel => TestModels.GetModelName(ModelType.Qwen352b);

	/// <summary>
	/// The client under test.
	/// </summary>
	protected OllamaClient OllamaClient { get; }

	/// <summary>
	/// Writes to the test's output at Debug level, so the client's request and response logging is
	/// visible in a failing test.
	/// </summary>
	protected ILogger Logger { get; }

	/// <summary>
	/// The running test's cancellation token.
	/// </summary>
	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	/// <summary>
	/// Creates the client the test will use, from the configured server address.
	/// </summary>
	/// <param name="fixture">The shared fixture that supplies configuration and services.</param>
	/// <param name="testOutputHelper">Where log output for the test is written.</param>
	/// <exception cref="InvalidOperationException">The test configuration is missing.</exception>
	public Test(Fixture fixture, ITestOutputHelper testOutputHelper) : base(testOutputHelper, fixture)
	{
		// Logger
		Logger = CreateLogger(testOutputHelper);

		// TestPortalConfig
		var testPortalConfigOptions = fixture
			.GetService<IOptions<TestConfig>>(testOutputHelper)
			?? throw new InvalidOperationException("TestPortalConfig is null");

		var testPortalConfig = testPortalConfigOptions.Value;

		OllamaClient = new OllamaClient(new OllamaClientOptions
		{
			Uri = new Uri(testPortalConfig.OllamaServer + ":" + testPortalConfig.OllamaPort),
			Logger = Logger
		});
	}
	private static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
	{
		// Create a logger factory with XUnit output
		var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder
				.AddProvider(new XunitLoggerProvider(testOutputHelper))
				.SetMinimumLevel(LogLevel.Debug);
		});

		return loggerFactory.CreateLogger("Ollama.Api.Test");
	}

	/// <summary>
	/// Disposes the client, then the test bed.
	/// </summary>
	public new async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		OllamaClient?.Dispose();

		await base.DisposeAsync();
	}
}