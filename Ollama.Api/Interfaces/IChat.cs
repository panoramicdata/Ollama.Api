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

	/// <summary>
	/// Generates a chat completion, yielding each chunk as it arrives (issue #1).
	/// </summary>
	/// <param name="chatRequest">
	/// The chat request. <see cref="ChatRequest.Stream"/> is forced on regardless of what the caller
	/// set, because a streaming method that quietly returned a single buffered object would be a
	/// trap.
	/// </param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The sequence of responses Ollama emits. Each one carries that chunk's delta in
	/// <see cref="ChatResponse.Message"/>; the last has <see cref="ChatResponse.Done"/> set, along
	/// with the timing fields. Consumers accumulate the deltas themselves.
	/// </returns>
	/// <remarks>
	/// <see cref="ChatAsync"/> remains the right call for a tool-calling loop, where nothing can be
	/// shown until the turn resolves anyway. This exists for the final turn, where the answer is
	/// long enough that a user waiting on it should be able to watch it arrive.
	/// </remarks>
	// The attribute is required only to satisfy Refit's analyser, which treats this whole interface
	// as Refit-generated because of the attribute above. Nothing generates IChat - ChatClient
	// implements it by hand - and this method never goes through Refit at all. Same endpoint, so the
	// attribute is at least not a lie.
	[Post("/api/chat")]
	IAsyncEnumerable<ChatResponse> ChatStreamAsync(
		ChatRequest chatRequest,
		CancellationToken cancellationToken);
}
