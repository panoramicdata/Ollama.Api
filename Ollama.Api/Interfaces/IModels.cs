using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Interface for model management endpoints (show, copy, delete, pull, push, create).
/// </summary>
public interface IModels
{
	[Post("/api/show")]
	Task<ShowModelResponse> ShowAsync(
		ShowModelRequest request,
		CancellationToken cancellationToken);

	[Post("/api/copy")]
	Task<ModelOperationResponse> CopyAsync(
		CopyModelRequest request,
		CancellationToken cancellationToken);

	[Post("/api/delete")]
	Task<ModelOperationResponse> DeleteAsync(
		DeleteModelRequest request,
		CancellationToken cancellationToken);

	[Post("/api/pull")]
	Task<ModelOperationResponse> PullAsync(
		PullModelRequest request,
		CancellationToken cancellationToken);

	[Post("/api/push")]
	Task<ModelOperationResponse> PushAsync(
		PushModelRequest request,
		CancellationToken cancellationToken);

	[Post("/api/create")]
	Task<ModelOperationResponse> CreateAsync(
		CreateModelRequest request,
		CancellationToken cancellationToken);

	[Post("/api/create")]
	Task<HttpResponseMessage> CreateStreamAsync(
		CreateModelRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a list of all models currently available in local Ollama storage.
	/// </summary>
	[Get("/api/tags")]
	Task<TagsResponse> GetTagsAsync(CancellationToken cancellationToken);
}
