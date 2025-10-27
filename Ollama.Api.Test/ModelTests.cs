using AwesomeAssertions;
using Ollama.Api.Models;

namespace Ollama.Api.Test;

public class ModelManagementTests(ITestOutputHelper testOutputHelper, Fixture fixture)
	: Test(fixture, testOutputHelper)
{
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
		
		// Act
		var act = async () => await OllamaClient.Models.ShowAsync(request, CancellationToken);
		
		// Assert
		var exception = await act.Should().ThrowAsync<Refit.ApiException>();
		exception.Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task DeleteModel_MissingModel_Returns405()
	{
		var request = new DeleteModelRequest { Name = "not-a-real-model:fake" };
		
		// Act
		var act = async () => await OllamaClient.Models.DeleteAsync(request, CancellationToken);
		
		// Assert
		var exception = await act.Should().ThrowAsync<Refit.ApiException>();
		exception.Which.StatusCode.Should().Be(System.Net.HttpStatusCode.MethodNotAllowed);
	}

	[Fact]
	public async Task CopyModel_MissingSource_Returns404()
	{
		var request = new CopyModelRequest { Source = "not-a-real-model:fake", Destination = "copy-of-not-a-real-model:fake" };
		
		// Act
		var act = async () => await OllamaClient.Models.CopyAsync(request, CancellationToken);
		
		// Assert
		var exception = await act.Should().ThrowAsync<Refit.ApiException>();
		exception.Which.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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
