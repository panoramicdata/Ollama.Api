using Ollama.Api.Models;
using Refit;

namespace Ollama.Api.Interfaces;

/// <summary>
/// The endpoints that report on the server itself rather than on any one model.
/// </summary>
public interface IUtility
{
	/// <summary>
	/// Gets the version of the Ollama server.
	/// </summary>
	[Get("/api/version")]
	Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Lists the models currently loaded into memory.
	/// </summary>
	[Get("/api/ps")]
	Task<PsResponse> GetPsAsync(CancellationToken cancellationToken);
}
