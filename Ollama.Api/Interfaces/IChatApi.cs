using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

internal interface IChatApi
{
	[Post("/api/chat")]
	Task<ApiResponse<ChatResponse>> ChatAsync(
		ChatRequest chatRequest,
		CancellationToken cancellationToken);
}
