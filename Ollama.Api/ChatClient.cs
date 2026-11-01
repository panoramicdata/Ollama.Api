using Ollama.Api.Interfaces;
using Ollama.Api.Models;
using Refit;

namespace Ollama.Api;

internal sealed class ChatClient(IChatApi chatApi, HttpClient httpClient) : IChat
{
	public async Task<ChatResponse> ChatAsync(ChatRequest chatRequest, CancellationToken cancellationToken)
	{
		var apiResponse = await chatApi.ChatAsync(chatRequest, cancellationToken);
		var response = apiResponse.Content ?? new ChatResponse();

		if (!apiResponse.IsSuccessStatusCode)
		{
			response.Error ??= DescribeFailure(apiResponse);
		}

		return response;
	}

	/// <summary>
	/// The most specific description of a failed call that the response carries: Ollama's own error
	/// message where there is one, then the reason phrase, then whatever Refit reported.
	/// </summary>
	private static string? DescribeFailure(IApiResponse<ChatResponse> apiResponse)
		=> apiResponse.Error is ApiException apiError
			? OllamaError.Extract(apiError.Content) ?? apiError.ReasonPhrase
			: apiResponse.Error?.Message;

	public IAsyncEnumerable<ChatResponse> ChatStreamAsync(
		ChatRequest chatRequest,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(chatRequest);

		return ChatStreamReader.ReadAsync(httpClient, ToStreamingRequest(chatRequest), cancellationToken);
	}

	/// <summary>
	/// A copy of the request with streaming turned on.
	/// </summary>
	/// <remarks>
	/// Copied rather than mutated: the caller's request object may be reused for a later
	/// non-streaming call, and quietly turning streaming on for that would be a surprising side effect.
	/// </remarks>
	internal static ChatRequest ToStreamingRequest(ChatRequest chatRequest) => new()
	{
		Model = chatRequest.Model,
		Messages = chatRequest.Messages,
		Options = chatRequest.Options,
		Think = chatRequest.Think,
		Logprobs = chatRequest.Logprobs,
		TopLogprobs = chatRequest.TopLogprobs,
		Tools = chatRequest.Tools,
		Format = chatRequest.Format,
		KeepAlive = chatRequest.KeepAlive,
		Stream = true
	};
}
