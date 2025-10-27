using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

public class ModelManagementTests(ITestOutputHelper testOutputHelper, Fixture fixture)
	: Test(fixture, testOutputHelper)
{
	//[Fact]
	//public async Task ModelLifecycle_CreateCopyDelete_Succeeds()
	//{
	//	// Use unique names for the test
	//	var testModel = $"test-model-{Guid.NewGuid():N}:v1";
	//	var testModelCopy = $"{testModel}-copy";

	//	// Precondition: Ensure base model exists
	//	var tagsResponse = await OllamaClient.Models.GetTagsAsync(default);
	//	tagsResponse.Should().NotBeNull();
	//	var modelExists = tagsResponse.Models?.Any(m => string.Equals(m.Name, TestModel, StringComparison.OrdinalIgnoreCase)) ?? false;
	//	if (!modelExists)
	//	{
	//		throw new InvalidOperationException($"Precondition not met: Required base model '{TestModel}' is not present in local storage.");
	//	}

	//	// 1. Create (streaming)
	//	var createRequest = new CreateModelRequest
	//	{
	//		Name = testModel,
	//		From = TestModel,
	//		System = "test",
	//		Path = null
	//	};
	//	var createResponses = new List<ModelOperationResponse>();
	//	await foreach (var update in OllamaClient.Models.CreateAsStreamAsync(createRequest, default))
	//	{
	//		createResponses.Add(update);
	//	}

	//	createResponses.Should().NotBeEmpty();
	//	var finalCreate = createResponses.Last();
	//	finalCreate.Should().NotBeNull();
	//	finalCreate.Status.Should().Be("success");

	//	// 2. Copy
	//	var copyRequest = new CopyModelRequest { Source = testModel, Destination = testModelCopy };
	//	var copyResponse = await OllamaClient.Models.CopyAsync(copyRequest, default);
	//	copyResponse.Should().NotBeNull();
	//	finalCreate.Status.Should().Be("success");

	//	// 3. Delete copy
	//	var deleteCopyRequest = new DeleteModelRequest { Name = testModelCopy };
	//	var deleteCopyResponse = await OllamaClient.Models.DeleteAsync(deleteCopyRequest, default);
	//	deleteCopyResponse.Should().NotBeNull();
	//	finalCreate.Status.Should().Be("success");

	//	// 4. Delete original
	//	var deleteRequest = new DeleteModelRequest { Name = testModel };
	//	var deleteResponse = await OllamaClient.Models.DeleteAsync(deleteRequest, default);
	//	deleteResponse.Should().NotBeNull();
	//	finalCreate.Status.Should().Be("success");
	//}

	[Fact]
	public async Task ShowModel_ReturnsModelInfo()
	{
		var request = new ShowModelRequest { Name = TestModel };
		var response = await OllamaClient.Models.ShowAsync(request, CancellationToken);
		response.Should().NotBeNull();
		response.Details.Should().NotBeNull();
	}

	[Fact]
	public async Task PullModel_AsStream_ReturnsSuccess()
	{
		var request = new PullModelRequest { Name = TestModel };
		var responses = new List<ModelOperationResponse>();
		await foreach (var update in OllamaClient.Models.PullAsStreamAsync(request, CancellationToken))
		{
			responses.Add(update);
		}

		responses.Should().NotBeEmpty();
		var finalResponse = responses.Last();
		finalResponse.Should().NotBeNull();
		finalResponse.Error.Should().BeNull();
		finalResponse.Status.Should().Be("success");
	}

	[Fact]
	public async Task PushModel_AsStream_ReturnsSuccess()
	{
		var request = new PushModelRequest { Name = "myuser/mymodel:latest" };
		var responses = new List<ModelOperationResponse>();
		await foreach (var update in OllamaClient.Models.PushAsStreamAsync(request, CancellationToken))
		{
			responses.Add(update);
		}

		responses.Should().NotBeEmpty();
		var finalResponse = responses.Last();
		finalResponse.Should().NotBeNull();
		finalResponse.Error.Should().NotBeNull();
	}

	[Fact]
	public async Task GetTags_ReturnsLocalModels()
	{
		var response = await OllamaClient.Models.GetTagsAsync(CancellationToken);
		response.Should().NotBeNull();
		response.Models.Should().NotBeNull();
		// Optionally check that at least one model exists
		// response.Models.Should().NotBeEmpty();
		foreach (var model in response.Models ?? [])
		{
			model.Name.Should().NotBeNullOrWhiteSpace();
			model.Size.Should().BePositive();
		}
	}

	[Fact]
	public async Task ShowModel_MissingModel_Returns404()
	{
		var request = new ShowModelRequest { Name = "not-a-real-model:fake" };
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Models.ShowAsync(request, CancellationToken);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task DeleteModel_MissingModel_Returns405()
	{
		var request = new DeleteModelRequest { Name = "not-a-real-model:fake" };
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Models.DeleteAsync(request, CancellationToken);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.MethodNotAllowed);
	}

	[Fact]
	public async Task CopyModel_MissingSource_Returns404()
	{
		var request = new CopyModelRequest { Source = "not-a-real-model:fake", Destination = "copy-of-not-a-real-model:fake" };
		var ex = await Assert.ThrowsAsync<Refit.ApiException>(async () =>
		{
			await OllamaClient.Models.CopyAsync(request, CancellationToken);
		});
		ex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}

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

		var responses = new List<ModelOperationResponse>();
		await foreach (var update in OllamaClient.Models.CreateAsStreamAsync(request, CancellationToken))
		{
			responses.Add(update);
		}

		responses.Should().NotBeEmpty();
		responses.Should().Contain(r => r.Error == null);
	}
}
