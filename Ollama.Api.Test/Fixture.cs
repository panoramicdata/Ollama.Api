using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ollama.Api.Test;

public class Fixture : TestBedFixture
{
	private IConfigurationRoot? _configuration;

	/// <summary>
	/// This method is used to add services to the service collection.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configuration"></param>
	protected override void AddServices(
	IServiceCollection services,
	IConfiguration? configuration)
	{
		if (_configuration is null)
		{
			throw new InvalidOperationException("Configuration is null");
		}

		services
		.AddScoped<CancellationTokenSource>()
		.Configure<TestConfig>(_configuration.GetSection("Config"));

		// Add a logger factory with minimum level Debug
		services.AddLogging(builder =>
		{
			builder.SetMinimumLevel(LogLevel.Debug);
		});
	}

	protected override ValueTask DisposeAsyncCore() => default;

	protected override IEnumerable<TestAppSettings> GetTestAppSettings()
	{
		_configuration = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddUserSecrets<Fixture>()
		.Build();

		return [
			new TestAppSettings
			{
				IsOptional = true,
				Filename = null,
			}
		];
	}
}
