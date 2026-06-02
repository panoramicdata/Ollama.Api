using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ollama.Api.Test;

public class Test : TestBed<Fixture>, IAsyncDisposable
{
	protected static string TestModel => TestModels.GetModelName(ModelType.Qwen352b);

	protected OllamaClient OllamaClient { get; }

	protected ILogger Logger { get; }

	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

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

	public new async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		OllamaClient?.Dispose();

		await base.DisposeAsync();
	}
}