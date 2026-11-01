using AwesomeAssertions;
using Ollama.Api.Models;
using Refit;
using System.Net;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// Model management, answered from responses Ollama 0.40.13 gave where it could be captured.
/// </summary>
public class ModelsOfflineTests
{
	private const string Digest = "sha256:0a109f422b47e3a30ba2b10eca18548e944e8a23073ee3f3e947efcf3c45e59f";

	/// <summary>
	/// A speech model reports the speech capability, its voices and its audio projector.
	/// </summary>
	[Fact]
	public async Task Show_SpeechModel_ReportsVoices()
	{
		var transport = new StubTransport().Then(Respond.Fixture("show-lfm3-audio.json"));
		using var client = Offline.Client(transport);

		var response = await client.Models.ShowAsync(new ShowModelRequest { Model = "lfm3-audio", Verbose = false }, Offline.Token);

		transport.LastRequest.Json["model"]!.GetValue<string>().Should().Be("lfm3-audio");
		transport.LastRequest.Json["verbose"]!.GetValue<bool>().Should().BeFalse();
		response.Capabilities.Should().Contain("speech");
		response.Voices.Should().HaveCount(39).And.Contain("female-british-english");
		response.Renderer.Should().Be("lfm3-audio");
		response.Parser.Should().Be("lfm3-audio");
		response.TemplateSource.Should().Be("renderer_parser");
		response.Requires.Should().Be("0.40.13");
		response.Details.Family.Should().Be("lfm3");
		response.Details.Runner.Should().Be("llamacpp");
		response.ProjectorInfo.Should().ContainKey("clip.has_gen_audio_encoder");
		response.ModelInfo!["lfm3.context_length"].Should().Be(128000L);
		response.License.Should().StartWith("LFM Open License");
		response.Modelfile.Should().StartWith("# Modelfile");
		response.Parameters.Should().Contain("temperature");
		response.Template.Should().Be("{{ .Prompt }}");
	}

	/// <summary>
	/// A decision model reports only the decision capability, and has no thinking controls.
	/// </summary>
	[Fact]
	public async Task Show_DecisionModel_HasNoThinking()
	{
		var transport = new StubTransport().Then(Respond.Fixture("show-d3.json"));
		using var client = Offline.Client(transport);

		var response = await client.Models.ShowAsync(new ShowModelRequest { Model = "d3:2b" }, Offline.Token);

		response.Capabilities.Should().Contain("decision");
		response.Thinking.Should().BeNull();
		response.Parser.Should().BeNull();
		response.Renderer.Should().Be("d3");
	}

	/// <summary>
	/// A thinking model lists the think values it accepts and its default.
	/// </summary>
	[Fact]
	public async Task Show_ThinkingModel_ListsThinkValues()
	{
		var transport = new StubTransport().Then(Respond.Fixture("show-qwen3.5.json"));
		using var client = Offline.Client(transport);

		var response = await client.Models.ShowAsync(new ShowModelRequest { Model = "qwen3.5:2b" }, Offline.Token);

		response.Thinking!.Values.Should().Equal(false, true);
		response.Thinking.Default.Should().Be(ThinkValue.FromBoolean(true));
		response.ModifiedAt.Year.Should().Be(2026);
	}

	/// <summary>
	/// The model list carries each model's digest and details.
	/// </summary>
	[Fact]
	public async Task GetTags_ReadsDigestAndDetails()
	{
		var transport = new StubTransport().Then(Respond.Fixture("tags.json"));
		using var client = Offline.Client(transport);

		var response = await client.Models.GetTagsAsync(Offline.Token);

		var model = response.Models!.Should().Contain(m => m.Name == "d3:2b").Which;
		model.Model.Should().Be("d3:2b");
		model.Digest.Should().HaveLength(64);
		model.Size.Should().BePositive();
		model.ModifiedAt.Should().NotBeNull();
		model.Details!.Family.Should().Be("qwen35");
		model.RemoteHost.Should().BeNull();
		model.RemoteModel.Should().BeNull();
	}

	/// <summary>
	/// A delete is sent as DELETE, and its empty success body is reported as success.
	/// </summary>
	[Fact]
	public async Task Delete_SendsDeleteAndReportsSuccess()
	{
		var transport = new StubTransport().Then(Respond.Empty(HttpStatusCode.OK));
		using var client = Offline.Client(transport);

		var response = await client.Models.DeleteAsync(new DeleteModelRequest { Model = "scratch:latest" }, Offline.Token);

		response.Status.Should().Be("success");
		transport.LastRequest.Method.Should().Be(HttpMethod.Delete);
		transport.LastRequest.Uri.AbsolutePath.Should().Be("/api/delete");
		transport.LastRequest.Json["model"]!.GetValue<string>().Should().Be("scratch:latest");
	}

	/// <summary>
	/// Deleting a model that does not exist throws with the status Ollama gave.
	/// </summary>
	[Fact]
	public async Task Delete_MissingModel_Throws()
	{
		var transport = new StubTransport().Then(Respond.Json(HttpStatusCode.NotFound, """{"error":"model 'x' not found"}"""));
		using var client = Offline.Client(transport);

		var act = () => client.Models.DeleteAsync(new DeleteModelRequest { Model = "x" }, Offline.Token);

		(await act.Should().ThrowAsync<ApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	/// <summary>
	/// The streamed delete is sent as DELETE too, and a successful delete streams nothing.
	/// </summary>
	[Fact]
	public async Task DeleteAsStream_SendsDelete()
	{
		var transport = new StubTransport().Then(Respond.Empty(HttpStatusCode.OK));
		using var client = Offline.Client(transport);

		var updates = await ToListAsync(client.Models.DeleteAsStreamAsync(new DeleteModelRequest { Model = "scratch:latest" }, Offline.Token));

		updates.Should().BeEmpty();
		transport.LastRequest.Method.Should().Be(HttpMethod.Delete);
	}

	/// <summary>
	/// A blob that exists is reported as present, and one that does not as absent.
	/// </summary>
	[Fact]
	public async Task BlobExists_ReadsPresenceFromTheStatus()
	{
		var transport = new StubTransport()
			.Then(Respond.Empty(HttpStatusCode.OK))
			.Then(Respond.Empty(HttpStatusCode.NotFound));
		using var client = Offline.Client(transport);

		(await client.Models.BlobExistsAsync(Digest, Offline.Token)).Should().BeTrue();
		(await client.Models.BlobExistsAsync(Digest, Offline.Token)).Should().BeFalse();

		transport.Requests.Should().AllSatisfy(r => r.Method.Should().Be(HttpMethod.Head));
		transport.LastRequest.Uri.AbsolutePath.Should().StartWith("/api/blobs/sha256");
	}

	/// <summary>
	/// A malformed digest is an error, not an absent blob.
	/// </summary>
	[Fact]
	public async Task BlobExists_BadDigest_Throws()
	{
		var transport = new StubTransport().Then(Respond.Empty(HttpStatusCode.BadRequest));
		using var client = Offline.Client(transport);

		await client.Invoking(c => c.Models.BlobExistsAsync("sha256:nope", Offline.Token))
			.Should().ThrowAsync<HttpRequestException>();
	}

	/// <summary>
	/// A blob is uploaded as the caller's bytes, and the caller's stream is left open.
	/// </summary>
	[Fact]
	public async Task PushBlob_UploadsTheBytesAndLeavesTheStreamOpen()
	{
		var transport = new StubTransport().Then(Respond.Empty(HttpStatusCode.Created));
		using var client = Offline.Client(transport);
		using var blob = new MemoryStream([1, 2, 3]);

		await client.Models.PushBlobAsync(Digest, blob, Offline.Token);

		transport.LastRequest.Method.Should().Be(HttpMethod.Post);
		transport.LastRequest.Body.Should().Equal(1, 2, 3);
		blob.CanRead.Should().BeTrue();
	}

	/// <summary>
	/// A blob whose digest does not match is refused.
	/// </summary>
	[Fact]
	public async Task PushBlob_Refused_Throws()
	{
		var transport = new StubTransport().Then(Respond.Empty(HttpStatusCode.BadRequest));
		using var client = Offline.Client(transport);
		using var blob = new MemoryStream([1]);

		await client.Invoking(c => c.Models.PushBlobAsync(Digest, blob, Offline.Token))
			.Should().ThrowAsync<HttpRequestException>();
	}

	/// <summary>
	/// A create request sends every field under the name Ollama gives it.
	/// </summary>
	[Fact]
	public async Task Create_SendsEveryField()
	{
		var transport = new StubTransport().Then(Respond.Json("""{"status":"success"}"""));
		using var client = Offline.Client(transport);

		var response = await client.Models.CreateAsync(new CreateModelRequest
		{
			Model = "triage",
			From = "d3:2b",
			System = "s",
			Template = "t",
			Renderer = "r",
			Parser = "p",
			Files = new() { ["model.gguf"] = Digest },
			DraftFiles = new() { ["draft.gguf"] = Digest },
			License = ["MIT"],
			Parameters = new() { ["temperature"] = 0 },
			Messages = [new ChatMessage { Role = "user", Content = "hi" }],
			Quantize = "q4_K_M",
			DraftQuantize = "q8_0",
			Requires = "0.40.9",
			Capabilities = ["decision"],
			Stream = false
		}, Offline.Token);

		response.Status.Should().Be("success");
		var json = transport.LastRequest.Json.AsObject();
		json.Select(p => p.Key).Should().Contain(
		[
			"model", "from", "system", "template", "renderer", "parser", "files", "draft_files", "license",
			"parameters", "messages", "quantize", "draft_quantize", "requires", "capabilities", "stream"
		]);
		json.Should().NotContainKey("name");
	}

	/// <summary>
	/// Pull, push and create stream their progress as newline-delimited JSON, skipping blank lines.
	/// </summary>
	[Fact]
	public async Task StreamedOperations_YieldEachUpdate()
	{
		const string Progress = "{\"status\":\"pulling manifest\"}\n\n{\"status\":\"success\"}\n";
		var transport = new StubTransport()
			.Then(Respond.Streamed("application/x-ndjson", Progress))
			.Then(Respond.Streamed("application/x-ndjson", Progress))
			.Then(Respond.Streamed("application/x-ndjson", Progress));
		using var client = Offline.Client(transport);

		var pulled = await ToListAsync(client.Models.PullAsStreamAsync(new PullModelRequest { Model = "d3:2b", Insecure = false, Stream = true, Digest = Digest }, Offline.Token));
		var pushed = await ToListAsync(client.Models.PushAsStreamAsync(new PushModelRequest { Model = "me/d3", Insecure = false, Stream = true }, Offline.Token));
		var created = await ToListAsync(client.Models.CreateAsStreamAsync(new CreateModelRequest { Model = "x", From = "d3:2b" }, Offline.Token));

		foreach (var updates in new[] { pulled, pushed, created })
		{
			updates.Select(u => u.Status).Should().Equal("pulling manifest", "success");
		}

		transport.Requests[0].Json["model"]!.GetValue<string>().Should().Be("d3:2b");
		transport.Requests[1].Json["model"]!.GetValue<string>().Should().Be("me/d3");
	}

	/// <summary>
	/// A streamed operation stops when its enumeration is cancelled.
	/// </summary>
	[Fact]
	public async Task StreamedOperation_Cancelled_Stops()
	{
		var transport = new StubTransport().Then(Respond.Streamed("application/x-ndjson", "{\"status\":\"a\"}\n{\"status\":\"b\"}\n"));
		using var client = Offline.Client(transport);
		using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Offline.Token);

		var updates = new List<ModelOperationResponse>();
		await foreach (var update in client.Models.PullAsStreamAsync(new PullModelRequest { Model = "d3:2b" }, cancellation.Token))
		{
			updates.Add(update);
			await cancellation.CancelAsync();
		}

		updates.Should().ContainSingle();
	}

	/// <summary>
	/// Copy and the buffered pull and push are posted to their endpoints.
	/// </summary>
	[Fact]
	public async Task BufferedOperations_PostToTheirEndpoints()
	{
		var transport = new StubTransport()
			.Then(Respond.Json("""{"status":"success"}"""))
			.Then(Respond.Json("""{"status":"success"}"""))
			.Then(Respond.Json("""{"status":"success"}"""));
		using var client = Offline.Client(transport);

		await client.Models.CopyAsync(new CopyModelRequest { Source = "a", Destination = "b" }, Offline.Token);
		await client.Models.PullAsync(new PullModelRequest { Model = "a" }, Offline.Token);
		await client.Models.PushAsync(new PushModelRequest { Model = "a" }, Offline.Token);

		transport.Requests.Select(r => r.Uri.AbsolutePath).Should().Equal("/api/copy", "/api/pull", "/api/push");
	}

	/// <summary>
	/// The obsolete name still sets the model, so code written against earlier versions keeps working.
	/// </summary>
	[Fact]
	public void ObsoleteName_SetsTheModel()
	{
#pragma warning disable CS0618 // The point of the test is the obsolete member.
		var request = new PullModelRequest { Name = "d3:2b" };

		request.Name.Should().Be("d3:2b");
#pragma warning restore CS0618
		request.Model.Should().Be("d3:2b");
	}

	private static async Task<List<ModelOperationResponse>> ToListAsync(IAsyncEnumerable<ModelOperationResponse> updates)
	{
		var list = new List<ModelOperationResponse>();
		await foreach (var update in updates)
		{
			list.Add(update);
		}

		return list;
	}
}
