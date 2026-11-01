using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Ollama cloud usage over a time range, from <c>/api/usage</c>.
/// </summary>
public class UsageResponse
{
	/// <summary>
	/// The range covered: <c>24h</c>, <c>7d</c> or <c>30d</c>.
	/// </summary>
	[JsonPropertyName("range")]
	public string? Range { get; set; }

	/// <summary>
	/// <c>self</c> or <c>team</c>.
	/// </summary>
	[JsonPropertyName("scope")]
	public string? Scope { get; set; }

	/// <summary>
	/// <c>hour</c> for a 24-hour range, otherwise <c>day</c>.
	/// </summary>
	[JsonPropertyName("granularity")]
	public string? Granularity { get; set; }

	/// <summary>
	/// <c>model</c> when usage was grouped by model.
	/// </summary>
	[JsonPropertyName("group_by")]
	public string? GroupBy { get; set; }

	/// <summary>
	/// The start of the range in UTC, inclusive.
	/// </summary>
	[JsonPropertyName("from")]
	public DateTimeOffset From { get; set; }

	/// <summary>
	/// The end of the range in UTC, exclusive.
	/// </summary>
	[JsonPropertyName("until")]
	public DateTimeOffset Until { get; set; }

	/// <summary>
	/// Usage across the whole range.
	/// </summary>
	[JsonPropertyName("totals")]
	public UsageSummary? Totals { get; set; }

	/// <summary>
	/// Usage per hour or day, oldest first, including empty buckets.
	/// </summary>
	[JsonPropertyName("buckets")]
	public List<UsageBucket> Buckets { get; set; } = [];
}

/// <summary>
/// Counts of cloud requests, tokens and their value.
/// </summary>
public class UsageMetrics
{
	/// <summary>
	/// The number of requests.
	/// </summary>
	[JsonPropertyName("request_count")]
	public long RequestCount { get; set; }

	/// <summary>
	/// The requests' value in US dollars, whether covered by the plan or paid from credits.
	/// </summary>
	[JsonPropertyName("usage_usd")]
	public decimal? UsageUsd { get; set; }

	/// <summary>
	/// Input tokens, including cached ones.
	/// </summary>
	[JsonPropertyName("input_tokens")]
	public long? InputTokens { get; set; }

	/// <summary>
	/// Input tokens read from cache, already counted in <see cref="InputTokens"/>.
	/// </summary>
	[JsonPropertyName("cached_input_tokens")]
	public long? CachedInputTokens { get; set; }

	/// <summary>
	/// Output tokens.
	/// </summary>
	[JsonPropertyName("output_tokens")]
	public long? OutputTokens { get; set; }
}

/// <summary>
/// One model's usage.
/// </summary>
public class UsageGroup : UsageMetrics
{
	/// <summary>
	/// The model's name.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }
}

/// <summary>
/// Usage in total, optionally broken down by model.
/// </summary>
public class UsageSummary : UsageMetrics
{
	/// <summary>
	/// Up to ten models' usage, when grouped by model.
	/// </summary>
	[JsonPropertyName("groups")]
	public List<UsageGroup>? Groups { get; set; }

	/// <summary>
	/// The combined usage of the models outside <see cref="Groups"/>.
	/// </summary>
	[JsonPropertyName("remainder")]
	public UsageMetrics? Remainder { get; set; }
}

/// <summary>
/// Usage in one hour or day.
/// </summary>
public class UsageBucket : UsageSummary
{
	/// <summary>
	/// The start of the bucket in UTC, inclusive.
	/// </summary>
	[JsonPropertyName("from")]
	public DateTimeOffset From { get; set; }

	/// <summary>
	/// The end of the bucket in UTC, exclusive.
	/// </summary>
	[JsonPropertyName("until")]
	public DateTimeOffset Until { get; set; }

	/// <summary>
	/// True for the current hour or day, which is still in progress.
	/// </summary>
	[JsonPropertyName("partial")]
	public bool? Partial { get; set; }
}

/// <summary>
/// The time range <c>/api/usage</c> reports on.
/// </summary>
public enum UsageRange
{
	/// <summary>
	/// The last 24 hours, by hour.
	/// </summary>
	Last24Hours,

	/// <summary>
	/// The last 7 days, by day.
	/// </summary>
	Last7Days,

	/// <summary>
	/// The last 30 days, by day.
	/// </summary>
	Last30Days
}
