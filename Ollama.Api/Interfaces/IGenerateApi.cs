using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// The Ollama Chat API endpoints
/// </summary>
public interface IGenerateApi
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