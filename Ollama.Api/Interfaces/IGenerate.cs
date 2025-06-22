using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Generates a text completion for a given prompt using a specified model.
/// This is used for single-turn text generation.
/// </summary>
public interface IGenerate
{
	/// <summary>
	/// Sends a message to the chat model and returns the response.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <returns>A task that represents the asynchronous operation, containing the chat response.</returns>
	[Post("/api/generate")]
	Task<GenerateResponse> GenerateAsync(
		GenerateRequest generateRequest,
		CancellationToken cancellationToken);
}