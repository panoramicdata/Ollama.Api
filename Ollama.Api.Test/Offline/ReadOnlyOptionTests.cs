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

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new OllamaClientOptions { Uri = new Uri("http://ollama.test:11434") }.IsReadOnly.Should().BeFalse();

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public async Task ReadOnly_DeleteModel_ThrowsBeforeSending()
	{
		var transport = new StubTransport();
		using var client = ClientFor(transport, isReadOnly: true);

		var act = () => client.Models.DeleteAsync(new DeleteModelRequest { Model = "m" }, Offline.Token);

		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).WithInnerException<InvalidOperationException>();
		transport.Requests.Should().BeEmpty();
	}

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public async Task ReadOnly_PullModel_ThrowsBeforeSending()
	{
		var transport = new StubTransport();
		using var client = ClientFor(transport, isReadOnly: true);

		var act = () => client.Models.PullAsync(new PullModelRequest { Model = "m" }, Offline.Token);

		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).WithInnerException<InvalidOperationException>();
		transport.Requests.Should().BeEmpty();
	}

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public async Task ReadOnly_Version_StillGoesThrough()
	{
		var transport = new StubTransport().Then(Respond.Json("""{"version":"0.40.13"}"""));
		using var client = ClientFor(transport, isReadOnly: true);

		await client.Utility.GetVersionAsync(Offline.Token);

		transport.Requests.Should().ContainSingle();
	}

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public async Task ReadOnly_Embed_PostByNameStillGoesThrough()
	{
		var transport = new StubTransport().Then(Respond.Json("""{"model":"m","embeddings":[[0.1]]}"""));
		using var client = ClientFor(transport, isReadOnly: true);

		await client.Embeddings.EmbedAsync(new EmbedRequest { Model = "m", Input = ["x"] }, Offline.Token);

		transport.Requests.Should().ContainSingle();
	}

	/// <summary>Read-only option behaviour.</summary>
	[Fact]
	public async Task NotReadOnly_DeleteModel_IsSent()
	{
		var transport = new StubTransport().Then(Respond.Empty(System.Net.HttpStatusCode.OK));
		using var client = ClientFor(transport, isReadOnly: false);

		await client.Models.DeleteAsync(new DeleteModelRequest { Model = "m" }, Offline.Token);

		transport.Requests.Should().ContainSingle();
	}
}
