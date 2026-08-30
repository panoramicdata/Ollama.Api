using AwesomeAssertions;

namespace Ollama.Api.Test;

/// <summary>
/// Exercises the server-level endpoints - version and ps - against a live Ollama server.
/// </summary>
/// <param name="testOutputHelper">Where log output for the test is written.</param>
/// <param name="fixture">The shared fixture that supplies configuration and services.</param>
public class UtilityTests(ITestOutputHelper testOutputHelper, Fixture fixture)
	: Test(fixture, testOutputHelper)
{
	/// <summary>
	/// The version endpoint reports a version string.
	/// </summary>
	[Fact]
	public async Task GetVersion_ReturnsVersionString()
	{
		var response = await OllamaClient.Utility.GetVersionAsync(CancellationToken);
		response.Should().NotBeNull();
		response.Version.Should().NotBeNullOrWhiteSpace();
	}

	/// <summary>
	/// The ps endpoint lists the loaded models, which may legitimately be an empty list.
	/// </summary>
	[Fact]
	public async Task GetPs_ReturnsRunningModels()
	{
		var response = await OllamaClient.Utility.GetPsAsync(CancellationToken);
		response.Should().NotBeNull();
		response.Models.Should().NotBeNull();
		// Models list may be empty if no models are running, but should not be null
		foreach (var model in response.Models ?? [])
		{
			model.Name.Should().NotBeNullOrWhiteSpace();
		}
	}
}
