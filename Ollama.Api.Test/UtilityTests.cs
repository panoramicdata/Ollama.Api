using AwesomeAssertions;
using Xunit.Abstractions;

namespace Ollama.Api.Test;

public class UtilityTests(ITestOutputHelper testOutputHelper, Fixture fixture)
	: Test(fixture, testOutputHelper)
{
	[Fact]
	public async Task GetVersion_ReturnsVersionString()
	{
		var response = await OllamaClient.Utility.GetVersionAsync();
		response.Should().NotBeNull();
		response.Version.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task GetPs_ReturnsRunningModels()
	{
		var response = await OllamaClient.Utility.GetPsAsync();
		response.Should().NotBeNull();
		response.Models.Should().NotBeNull();
		// Models list may be empty if no models are running, but should not be null
		foreach (var model in response.Models ?? [])
		{
			model.Name.Should().NotBeNullOrWhiteSpace();
		}
	}
}
