using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The remaining Ollama cloud credits, from <c>/api/balance</c>.
/// </summary>
public class BalanceResponse
{
	/// <summary>
	/// The credits included with the plan.
	/// </summary>
	[JsonPropertyName("included")]
	public IncludedBalance? Included { get; set; }

	/// <summary>
	/// The credits bought separately.
	/// </summary>
	[JsonPropertyName("purchased")]
	public PurchasedBalance? Purchased { get; set; }
}

/// <summary>
/// Plan credits: a dollar allowance per period, or, on a legacy plan, session and weekly limits.
/// </summary>
public class IncludedBalance
{
	/// <summary>
	/// The remaining allowance in US dollars. Null on a legacy plan.
	/// </summary>
	[JsonPropertyName("balance_usd")]
	public decimal? BalanceUsd { get; set; }

	/// <summary>
	/// The allowance for the whole period in US dollars. Null on a legacy plan.
	/// </summary>
	[JsonPropertyName("allowance_usd")]
	public decimal? AllowanceUsd { get; set; }

	/// <summary>
	/// The period the allowance covers. Null on a legacy plan.
	/// </summary>
	[JsonPropertyName("period")]
	public BalancePeriod? Period { get; set; }

	/// <summary>
	/// The per-session limit, on a legacy plan.
	/// </summary>
	[JsonPropertyName("session")]
	public BalanceLimit? Session { get; set; }

	/// <summary>
	/// The weekly limit, on a legacy plan.
	/// </summary>
	[JsonPropertyName("weekly")]
	public BalanceLimit? Weekly { get; set; }
}

/// <summary>
/// The period an included allowance covers.
/// </summary>
public class BalancePeriod
{
	/// <summary>
	/// The start of the period in UTC, inclusive.
	/// </summary>
	[JsonPropertyName("from")]
	public DateTimeOffset From { get; set; }

	/// <summary>
	/// The end of the period in UTC, exclusive.
	/// </summary>
	[JsonPropertyName("until")]
	public DateTimeOffset Until { get; set; }
}

/// <summary>
/// A legacy plan limit.
/// </summary>
public class BalanceLimit
{
	/// <summary>
	/// The percentage of the limit left, from 0 to 100.
	/// </summary>
	[JsonPropertyName("remaining_percent")]
	public double RemainingPercent { get; set; }

	/// <summary>
	/// When the limit next resets, in UTC.
	/// </summary>
	[JsonPropertyName("resets_at")]
	public DateTimeOffset ResetsAt { get; set; }
}

/// <summary>
/// Credits bought separately from the plan.
/// </summary>
public class PurchasedBalance
{
	/// <summary>
	/// The unexpired purchased credits in US dollars.
	/// </summary>
	[JsonPropertyName("balance_usd")]
	public decimal BalanceUsd { get; set; }
}
