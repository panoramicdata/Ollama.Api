using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

public interface IUtility
{
	[Get("/api/version")]
	Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken);

	[Get("/api/ps")]
	Task<PsResponse> GetPsAsync(CancellationToken cancellationToken);
}
