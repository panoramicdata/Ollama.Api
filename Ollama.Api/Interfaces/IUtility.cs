using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

public interface IUtility
{
	[Get("/api/version")]
	Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken = default);

	[Get("/api/ps")]
	Task<PsResponse> GetPsAsync(CancellationToken cancellationToken = default);
}
