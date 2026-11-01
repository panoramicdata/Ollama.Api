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

	/// <summary>
	/// Gets the remaining Ollama cloud credits. Needs a server signed in to ollama.com, or an API key
	/// against ollama.com itself.
	/// </summary>
	[Get("/api/balance")]
	Task<BalanceResponse> GetBalanceAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets your own Ollama cloud usage over the last seven days. Needs a server signed in to
	/// ollama.com, or an API key against ollama.com itself.
	/// </summary>
	Task<UsageResponse> GetUsageAsync(CancellationToken cancellationToken)
		=> GetUsageAsync(new UsageQuery(), cancellationToken);

	/// <summary>
	/// Gets Ollama cloud usage for the range, scope and grouping in <paramref name="query"/>.
	/// </summary>
	Task<UsageResponse> GetUsageAsync(UsageQuery query, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(query);

		return GetUsageInternalAsync(
			query.Range switch
			{
				UsageRange.Last24Hours => "24h",
				UsageRange.Last30Days => "30d",
				_ => "7d"
			},
			query.Team ? "team" : "self",
			query.GroupByModel ? "model" : null,
			cancellationToken);
	}

	/// <summary>
	/// Gets cloud usage with the query values already in their wire form. Use <see cref="GetUsageAsync(UsageQuery, CancellationToken)"/>.
	/// </summary>
	[Get("/api/usage")]
	Task<UsageResponse> GetUsageInternalAsync(
		[AliasAs("range")] string range,
		[AliasAs("scope")] string scope,
		[AliasAs("group_by")] string? groupBy,
		CancellationToken cancellationToken);
}
