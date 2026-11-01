namespace Ollama.Api.Models;

/// <summary>
/// What a cloud usage report covers.
/// </summary>
public class UsageQuery
{
	/// <summary>
	/// The time range. Defaults to the last seven days.
	/// </summary>
	public UsageRange Range { get; set; } = UsageRange.Last7Days;

	/// <summary>
	/// Whether to report the whole team's usage rather than your own. Needs team admin access.
	/// </summary>
	public bool Team { get; set; }

	/// <summary>
	/// Whether to break the usage down by model.
	/// </summary>
	public bool GroupByModel { get; set; }
}
