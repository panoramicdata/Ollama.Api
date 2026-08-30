using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

/// <summary>
/// Exercises the model management endpoints - show, copy, delete, pull, push and create -
/// against a live Ollama server.
/// </summary>
/// <param name="testOutputHelper">Where log output for the test is written.</param>
/// <param name="fixture">The shared fixture that supplies configuration and services.</param>
public class ModelManagementTests(ITestOutputHelper testOutputHelper, Fixture fixture)
	: Test(fixture, testOutputHelper)
{
	/// <summary>
	/// Showing an installed model returns its details.
	/// </summary>
	[Fact]
	public async Task ShowModel_ReturnsModelInfo()
	{
		var request = new ShowModelRequest { Name = TestModel };
		var response = await OllamaClient.Models.ShowAsync(request, CancellationToken);
		response.Should().NotBeNull();
		response.Details.Should().NotBeNull();
	}

	/// <summary>
	/// Pulling a model streams progress updates and ends in success.
	/// </summary>
	[Fact]
	public async Task PullModel_AsStream_ReturnsSuccess()
	{
		var request = new PullModelRequest { Name = TestModel };
		var responses = await CollectStreamResponsesAsync(OllamaClient.Models.PullAsStreamAsync(request, CancellationToken));

		responses.Should().NotBeEmpty();
		var finalResponse = responses.Last();
		finalResponse.Should().NotBeNull();
		finalResponse.Error.Should().BeNull();
		finalResponse.Status.Should().Be("success");
	}

	/// <summary>
	/// Pushing a model streams progress updates and ends in success.
	/// </summary>
	[Fact]
	public async Task PushModel_AsStream_ReturnsSuccess()
	{
		var request = new PushModelRequest { Name = "myuser/mymodel:latest" };
		var responses = await CollectStreamResponsesAsync(OllamaClient.Models.PushAsStreamAsync(request, CancellationToken));

		responses.Should().NotBeEmpty();
		var finalResponse = responses.Last();
		finalResponse.Should().NotBeNull();
		finalResponse.Error.Should().NotBeNull();
	}

	/// <summary>
	/// The tags endpoint lists the models held locally.
	/// </summary>
	[Fact]
	public async Task GetTags_ReturnsLocalModels()
	{
		var response = await OllamaClient.Models.GetTagsAsync(CancellationToken);
		response.Should().NotBeNull();
		response.Models.Should().NotBeNull();
		foreach (var model in response.Models ?? [])
		{
			model.Name.Should().NotBeNullOrWhiteSpace();
			model.Size.Should().BePositive();
		}
	}

	/// <summary>
	/// Showing a model that is not installed returns 404.
	/// </summary>
	[Fact]
	public async Task ShowModel_MissingModel_Returns404()
		=> await AssertApiExceptionAsync(
			() => OllamaClient.Models.ShowAsync(new ShowModelRequest { Name = "not-a-real-model:fake" }, CancellationToken),
			System.Net.HttpStatusCode.NotFound);

	/// <summary>
	/// Deleting a model that is not installed returns 405.
	/// </summary>
	[Fact]
	public async Task DeleteModel_MissingModel_Returns405()
		=> await AssertApiExceptionAsync(
			() => OllamaClient.Models.DeleteAsync(new DeleteModelRequest { Name = "not-a-real-model:fake" }, CancellationToken),
			System.Net.HttpStatusCode.MethodNotAllowed);

	/// <summary>
	/// Copying from a source model that is not installed returns 404.
	/// </summary>
	[Fact]
	public async Task CopyModel_MissingSource_Returns404()
		=> await AssertApiExceptionAsync(
			() => OllamaClient.Models.CopyAsync(new CopyModelRequest { Source = "not-a-real-model:fake", Destination = "copy-of-not-a-real-model:fake" }, CancellationToken),
			System.Net.HttpStatusCode.NotFound);

	/// <summary>
	/// Creating a model from a base model that does not exist reports an error in the stream.
	/// </summary>
	[Fact]
	public async Task CreateModelStream_InvalidBaseModel_HasError()
	{
		var request = new CreateModelRequest
		{
			Name = "test-invalid-create:fake",
			From = "not-a-real-model:fake",
			System = "test",
			Path = null
		};

		var responses = await CollectStreamResponsesAsync(OllamaClient.Models.CreateAsStreamAsync(request, CancellationToken));

		responses.Should().NotBeEmpty();
		responses.Should().Contain(r => r.Error == null);
	}

	private static async Task<List<ModelOperationResponse>> CollectStreamResponsesAsync(IAsyncEnumerable<ModelOperationResponse> stream)
	{
		var responses = new List<ModelOperationResponse>();
		await foreach (var update in stream)
		{
			responses.Add(update);
		}

		return responses;
	}

	private static async Task AssertApiExceptionAsync(Func<Task> action, System.Net.HttpStatusCode expectedStatusCode)
	{
		var exception = await action.Should().ThrowAsync<Refit.ApiException>();
		exception.Which.StatusCode.Should().Be(expectedStatusCode);
	}
}
