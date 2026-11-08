using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// <see cref="OllamaClientOptions.IsReadOnly"/> refuses a request that would change the server before it is sent.
/// </summary>
public class ReadOnlyOptionTests
{
	private static OllamaClient ClientFor(StubTransport transport, bool isReadOnly)
		=> new(new OllamaClientOptions { Uri = new Uri("http://ollama.test:11434"), IsReadOnly = isReadOnly }, transport);

	private static async Task AssertRefusedAsync(Func<OllamaClient, Task> call)
	{
		var transport = new StubTransport();
		using var client = ClientFor(transport, isReadOnly: true);

		(await FluentActions.Awaiting(() => call(client)).Should().ThrowAsync<Refit.ApiRequestException>())
			.WithInnerException<InvalidOperationException>();
		transport.Requests.Should().BeEmpty();
	}

	private static async Task AssertSentAsync(bool isReadOnly, HttpResponseMessage response, Func<OllamaClient, Task> call)
	{
		var transport = new StubTransport().Then(response);
		using var client = ClientFor(transport, isReadOnly);

		await call(client);

		transport.Requests.Should().ContainSingle();
	}

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new OllamaClientOptions { Uri = new Uri("http://ollama.test:11434") }.IsReadOnly.Should().BeFalse();

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public Task ReadOnly_DeleteModel_ThrowsBeforeSending()
		=> AssertRefusedAsync(client => client.Models.DeleteAsync(new DeleteModelRequest { Model = "m" }, Offline.Token));

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public Task ReadOnly_PullModel_ThrowsBeforeSending()
		=> AssertRefusedAsync(client => client.Models.PullAsync(new PullModelRequest { Model = "m" }, Offline.Token));

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public Task ReadOnly_Version_StillGoesThrough()
		=> AssertSentAsync(true, Respond.Json("""{"version":"0.40.13"}"""), client => client.Utility.GetVersionAsync(Offline.Token));

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public Task ReadOnly_Embed_PostByNameStillGoesThrough()
		=> AssertSentAsync(
			true,
			Respond.Json("""{"model":"m","embeddings":[[0.1]]}"""),
			client => client.Embeddings.EmbedAsync(new EmbedRequest { Model = "m", Input = ["x"] }, Offline.Token));

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public Task NotReadOnly_DeleteModel_IsSent()
		=> AssertSentAsync(
			false,
			Respond.Empty(System.Net.HttpStatusCode.OK),
			client => client.Models.DeleteAsync(new DeleteModelRequest { Model = "m" }, Offline.Token));
}
