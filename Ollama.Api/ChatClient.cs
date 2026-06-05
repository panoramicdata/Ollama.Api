using Ollama.Api.Interfaces;
using Ollama.Api.Models;
using Refit;
using System.Text.Json;

namespace Ollama.Api;

internal sealed class ChatClient(IChatApi chatApi) : IChat
{
	public async Task<ChatResponse> ChatAsync(ChatRequest chatRequest, CancellationToken cancellationToken)
	{
		var apiResponse = await chatApi.ChatAsync(chatRequest, cancellationToken);
		if (apiResponse.IsSuccessStatusCode)
		{
			return apiResponse.Content ?? new ChatResponse();
		}

		var response = apiResponse.Content ?? new ChatResponse();
		var apiError = apiResponse.Error as ApiException;
		response.Error ??= TryGetErrorMessage(apiError) ?? apiError?.ReasonPhrase ?? apiResponse.Error?.Message;
		return response;
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
