using Ollama.Api.Interfaces;
using Ollama.Api.Models;
using Refit;
using System.Text.Json;

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
	{
		var apiError = apiResponse.Error as ApiException;

		return TryGetErrorMessage(apiError) ?? apiError?.ReasonPhrase ?? apiResponse.Error?.Message;
	}

	public IAsyncEnumerable<ChatResponse> ChatStreamAsync(
		ChatRequest chatRequest,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(chatRequest);

		// Copied rather than mutated: the caller's request object may be reused for a later
		// non-streaming call, and quietly turning streaming on for that would be a surprising
		// side effect.
		var streamingRequest = new ChatRequest
		{
			Model = chatRequest.Model,
			Messages = chatRequest.Messages,
			Options = chatRequest.Options,
			Tools = chatRequest.Tools,
			Format = chatRequest.Format,
			KeepAlive = chatRequest.KeepAlive,
			Stream = true
		};

		return ChatStreamReader.ReadAsync(httpClient, streamingRequest, cancellationToken);
	}

	private static string? TryGetErrorMessage(ApiException? exception)
	{
		if (exception is null || string.IsNullOrWhiteSpace(exception.Content))
		{
			return null;
		}

		try
		{
			using var jsonDocument = JsonDocument.Parse(exception.Content);
			if (jsonDocument.RootElement.TryGetProperty("error", out var errorProperty) && errorProperty.ValueKind == JsonValueKind.String)
			{
				return errorProperty.GetString();
			}
		}
		catch (JsonException)
		{
			return exception.Content;
		}

		return exception.Content;
	}
}
