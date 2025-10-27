using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Interface for the chat API, supporting conversational interactions.
/// </summary>
public interface IChat
{
	/// <summary>
	/// Generates a chat completion for a conversation.
	/// </summary>
	/// <param name="chatRequest">The chat request containing model, messages, and options.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation, containing the chat response.</returns>
	[Post("/api/chat")]
	Task<ChatResponse> ChatAsync(
		ChatRequest chatRequest,
		CancellationToken cancellationToken);
}
