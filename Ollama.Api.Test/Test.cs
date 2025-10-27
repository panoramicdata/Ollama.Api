using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ollama.Api.Test;

public class Test : TestBed<Fixture>, IAsyncDisposable
{
	protected const string TestModel = "llama3:latest";

	private readonly TestConfig _testPortalConfig;
	protected OllamaClient OllamaClient { get; }

	protected ILogger Logger { get; }

	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	public Test(Fixture fixture, ITestOutputHelper testOutputHelper) : base(testOutputHelper, fixture)
	{
		// Logger
		var loggerFactory = fixture.GetService<ILoggerFactory>(testOutputHelper) ?? throw new InvalidOperationException("LoggerFactory is null");
		Logger = loggerFactory.CreateLogger(GetType());

		// TestPortalConfig
		var testPortalConfigOptions = fixture
			.GetService<IOptions<TestConfig>>(testOutputHelper)
			?? throw new InvalidOperationException("TestPortalConfig is null");

		_testPortalConfig = testPortalConfigOptions.Value;

		OllamaClient = new OllamaClient(new OllamaClientOptions
		{
			Uri = new Uri(_testPortalConfig.OllamaServer + ":" + _testPortalConfig.OllamaPort),
			Logger = Logger
		});
	}

	public new async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		OllamaClient?.Dispose();

		await base.DisposeAsync();
	}
}
