using AwesomeAssertions;
using Ollama.Api.Models;
using Refit;
using System.Net;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// The server endpoints: version, loaded models, and cloud usage and balance.
/// </summary>
/// <remarks>
/// The usage and balance bodies follow the shapes in Ollama's OpenAPI document; the test servers are
/// not signed in to ollama.com, so no real response could be captured.
/// </remarks>
public class UtilityOfflineTests
{
	private const string Usage = """
		{"range":"24h","scope":"team","granularity":"hour","group_by":"model",
		 "from":"2026-10-31T17:00:00Z","until":"2026-11-01T17:00:00Z",
		 "totals":{"request_count":3,"usage_usd":0.12,"input_tokens":900,"cached_input_tokens":100,"output_tokens":40,
		   "groups":[{"model":"clef","request_count":2,"usage_usd":0.1}],
		   "remainder":{"request_count":1}},
		 "buckets":[{"from":"2026-11-01T16:00:00Z","until":"2026-11-01T17:00:00Z","partial":true,"request_count":3}]}
		""";

	/// <summary>
	/// The loaded models report their memory use, context length and runner.
	/// </summary>
	[Fact]
	public async Task GetPs_ReadsMemoryAndContext()
	{
		var transport = new StubTransport().Then(Respond.Fixture("ps.json"));
		using var client = Offline.Client(transport);

		var model = (await client.Utility.GetPsAsync(Offline.Token)).Models.Should().ContainSingle().Which;

		model.Name.Should().Be("qwen3.5:2b");
		model.Model.Should().Be("qwen3.5:2b");
		model.Size.Should().Be(2670953756);
		model.SizeVram.Should().Be(model.Size, "the model was entirely in GPU memory");
		model.ContextLength.Should().Be(32768);
		model.ExpiresAt.Should().NotBeNull();
		model.Digest.Should().HaveLength(64);
		model.Details!.QuantizationLevel.Should().Be("Q8_0");
		model.Runner.Should().Be("llamacpp");
	}

	/// <summary>
	/// The version endpoint reports the server's version.
	/// </summary>
	[Fact]
	public async Task GetVersion_ReadsTheVersion()
	{
		var transport = new StubTransport().Then(Respond.Json("""{"version":"0.40.13"}"""));
		using var client = Offline.Client(transport);

		(await client.Utility.GetVersionAsync(Offline.Token)).Version.Should().Be("0.40.13");
	}

	/// <summary>
	/// Each usage query option reaches the query string in its wire form.
	/// </summary>
	[Fact]
	public async Task GetUsage_SendsTheQuery()
	{
		var transport = new StubTransport()
			.Then(Respond.Json(Usage))
			.Then(Respond.Json(Usage))
			.Then(Respond.Json(Usage));
		using var client = Offline.Client(transport);

		await client.Utility.GetUsageAsync(Offline.Token);
		await client.Utility.GetUsageAsync(new UsageQuery { Range = UsageRange.Last24Hours, Team = true, GroupByModel = true }, Offline.Token);
		await client.Utility.GetUsageAsync(new UsageQuery { Range = UsageRange.Last30Days }, Offline.Token);

		transport.Requests.Select(r => r.Uri.Query).Should().Equal(
			"?range=7d&scope=self",
			"?range=24h&scope=team&group_by=model",
			"?range=30d&scope=self");
	}

	/// <summary>
	/// A usage report is read with its totals, groups and buckets.
	/// </summary>
	[Fact]
	public async Task GetUsage_ReadsTotalsGroupsAndBuckets()
	{
		var transport = new StubTransport().Then(Respond.Json(Usage));
		using var client = Offline.Client(transport);

		var usage = await client.Utility.GetUsageAsync(Offline.Token);

		usage.Range.Should().Be("24h");
		usage.Scope.Should().Be("team");
		usage.Granularity.Should().Be("hour");
		usage.GroupBy.Should().Be("model");
		usage.Until.Should().BeAfter(usage.From);
		usage.Totals!.RequestCount.Should().Be(3);
		usage.Totals.UsageUsd.Should().Be(0.12m);
		usage.Totals.InputTokens.Should().Be(900);
		usage.Totals.CachedInputTokens.Should().Be(100);
		usage.Totals.OutputTokens.Should().Be(40);
		usage.Totals.Groups.Should().ContainSingle().Which.Model.Should().Be("clef");
		usage.Totals.Remainder!.RequestCount.Should().Be(1);
		var bucket = usage.Buckets.Should().ContainSingle().Which;
		bucket.Partial.Should().BeTrue();
		bucket.Until.Should().BeAfter(bucket.From);
	}

	/// <summary>
	/// A missing usage query is refused before anything is sent.
	/// </summary>
	[Fact]
	public async Task GetUsage_NullQuery_IsRefused()
	{
		using var client = Offline.Client(new StubTransport());

		await client.Invoking(c => c.Utility.GetUsageAsync(null!, Offline.Token)).Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// A balance is read in the current shape, a dollar allowance per period.
	/// </summary>
	[Fact]
	public async Task GetBalance_ReadsTheAllowance()
	{
		var transport = new StubTransport().Then(Respond.Json("""
			{"included":{"balance_usd":4.5,"allowance_usd":20,"period":{"from":"2026-11-01T00:00:00Z","until":"2026-12-01T00:00:00Z"}},
			 "purchased":{"balance_usd":10}}
			"""));
		using var client = Offline.Client(transport);

		var balance = await client.Utility.GetBalanceAsync(Offline.Token);

		balance.Included!.BalanceUsd.Should().Be(4.5m);
		balance.Included.AllowanceUsd.Should().Be(20m);
		balance.Included.Period!.Until.Should().BeAfter(balance.Included.Period.From);
		balance.Purchased!.BalanceUsd.Should().Be(10m);
	}

	/// <summary>
	/// A balance is read in the legacy shape, session and weekly limits.
	/// </summary>
	[Fact]
	public async Task GetBalance_ReadsLegacyLimits()
	{
		var transport = new StubTransport().Then(Respond.Json("""
			{"included":{"session":{"remaining_percent":80,"resets_at":"2026-11-01T20:00:00Z"},
			             "weekly":{"remaining_percent":55.5,"resets_at":"2026-11-03T00:00:00Z"}},
			 "purchased":{"balance_usd":0}}
			"""));
		using var client = Offline.Client(transport);

		var balance = await client.Utility.GetBalanceAsync(Offline.Token);

		balance.Included!.BalanceUsd.Should().BeNull();
		balance.Included.Session!.RemainingPercent.Should().Be(80);
		balance.Included.Weekly!.RemainingPercent.Should().Be(55.5);
		balance.Included.Weekly.ResetsAt.Day.Should().Be(3);
	}

	/// <summary>
	/// A server not signed in to ollama.com refuses with 401, as the test servers do.
	/// </summary>
	[Fact]
	public async Task GetBalance_NotSignedIn_Throws()
	{
		var transport = new StubTransport().Then(Respond.Json(HttpStatusCode.Unauthorized, """{"error":"invalid credentials"}"""));
		using var client = Offline.Client(transport);

		var act = () => client.Utility.GetBalanceAsync(Offline.Token);

		(await act.Should().ThrowAsync<ApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
