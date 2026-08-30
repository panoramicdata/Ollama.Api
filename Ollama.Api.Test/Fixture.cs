using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ollama.Api.Test;

/// <summary>
/// The test bed fixture: reads the test configuration from user secrets and registers it.
/// </summary>
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
	}

	/// <summary>
	/// Nothing to release: the fixture holds only configuration.
	/// </summary>
	protected override ValueTask DisposeAsyncCore() => default;

	/// <summary>
	/// Builds the configuration from user secrets. No settings file is required, so that the server
	/// address can be kept out of the repository.
	/// </summary>
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
