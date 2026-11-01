using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// Decision models, which classify, answer yes or no, or score, returning probabilities rather than text.
/// </summary>
public interface IDecisions
{
	/// <summary>
	/// Answers every question in the request about its shared state, through <c>/v1/systemone</c>.
	/// </summary>
	/// <param name="request">The decision model, the state and the questions.</param>
	/// <param name="cancellationToken">Cancels the call.</param>
	/// <exception cref="ApiException">
	/// The model is not a decision model or is not installed, the request is too large, or a cloud
	/// model needs signing in or credit.
	/// </exception>
	[Post("/v1/systemone")]
	Task<DecisionResponse> DecideAsync(
		DecisionRequest request,
		CancellationToken cancellationToken);
}
