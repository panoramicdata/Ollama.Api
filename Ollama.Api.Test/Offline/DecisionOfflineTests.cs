using AwesomeAssertions;
using Ollama.Api.Models;
using Refit;
using System.Net;
using System.Text.Json;

namespace Ollama.Api.Test.Offline;

/// <summary>
/// Decision models through <c>/v1/systemone</c>, answered from responses d3:2b gave Ollama 0.40.13.
/// </summary>
public class DecisionOfflineTests
{
	private static DecisionRequest MixedRequest() => new()
	{
		Model = "d3:2b",
		State = new { ticket = "I was charged twice. Please refund the extra payment." },
		Questions = new()
		{
			["refund"] = new NoulQuestion
			{
				Instructions = "Is the customer requesting a refund?",
				Criteria = new NoulCriteria { False = "No refund is requested", True = "The customer requests a refund" }
			},
			["urgency"] = new ScoreQuestion
			{
				Instructions = "How urgently does this ticket need a response?",
				Criteria = ["Routine: no time pressure", "Soon: a customer is inconvenienced", "Immediate: a critical service is unavailable"]
			},
			["team"] = new ChoiceQuestion
			{
				Instructions = "Which team?",
				Criteria = new() { ["billing"] = null, ["technical"] = null }
			}
		},
		KeepAlive = "5m"
	};

	/// <summary>
	/// Each question type is sent with its type and its own shape of criteria, and the state object as JSON.
	/// </summary>
	[Fact]
	public async Task Decide_SendsEachQuestionTypeInItsOwnShape()
	{
		var transport = new StubTransport().Then(Respond.Fixture("systemone-mixed.json"));
		using var client = Offline.Client(transport);

		await client.Decisions.DecideAsync(MixedRequest(), Offline.Token);

		var sent = transport.LastRequest;
		sent.Method.Should().Be(HttpMethod.Post);
		sent.Uri.AbsolutePath.Should().Be("/v1/systemone");

		var json = sent.Json;
		json["state"]!["ticket"]!.GetValue<string>().Should().StartWith("I was charged twice");
		json["questions"]!["refund"]!["type"]!.GetValue<string>().Should().Be("noul");
		json["questions"]!["refund"]!["criteria"]!["true"]!.GetValue<string>().Should().Be("The customer requests a refund");
		json["questions"]!["urgency"]!["type"]!.GetValue<string>().Should().Be("score");
		json["questions"]!["urgency"]!["criteria"]!.AsArray().Should().HaveCount(3);
		json["questions"]!["team"]!["type"]!.GetValue<string>().Should().Be("choice");
		json["questions"]!["team"]!["criteria"]!.AsObject().Should().ContainKey("billing");
		json["keep_alive"]!.GetValue<string>().Should().Be("5m");
	}

	/// <summary>
	/// The answers to all three question types are read into their own answer types.
	/// </summary>
	[Fact]
	public async Task Decide_ReadsEachAnswerType()
	{
		var transport = new StubTransport().Then(Respond.Fixture("systemone-mixed.json"));
		using var client = Offline.Client(transport);

		var response = await client.Decisions.DecideAsync(MixedRequest(), Offline.Token);

		response.Model.Should().Be("d3:2b");
		response.Usage!.InputTokens.Should().Be(335);
		response.Usage.OutputTokens.Should().Be(0);

		response.Answers["refund"].Should().BeOfType<NoulAnswer>().Which.Noul.Should().BeApproximately(0.99907, 0.00001);

		var urgency = response.Answers["urgency"].Should().BeOfType<ScoreAnswer>().Which;
		urgency.Score.Should().BeApproximately(0.80278, 0.00001);
		urgency.Legend["1"].Should().Be("Soon: a customer is inconvenienced");
		urgency.Probabilities.Should().HaveCount(3);
		urgency.Confidence.Should().BeApproximately(0.49434, 0.00001);

		var team = response.Answers["team"].Should().BeOfType<ChoiceAnswer>().Which;
		team.Choice.Should().Be("billing");
		team.Probabilities["technical"].Should().BeApproximately(0.00089, 0.00001);
		team.Confidence.Should().BeApproximately(0.98965, 0.00001);
	}

	/// <summary>
	/// A string state is sent as a string, and a choice answer is read.
	/// </summary>
	[Fact]
	public async Task Decide_StringState_IsSentAsAString()
	{
		var transport = new StubTransport().Then(Respond.Fixture("systemone-choice.json"));
		using var client = Offline.Client(transport);

		var response = await client.Decisions.DecideAsync(new DecisionRequest
		{
			Model = "d3:2b",
			State = "Our checkout has returned 500 errors since 9am.",
			Questions = new()
			{
				["label"] = new ChoiceQuestion
				{
					Instructions = "Which label fits this ticket?",
					Criteria = new() { ["billing"] = "Payments and refunds", ["bug"] = "Software errors", ["account"] = "Login and account access" }
				}
			},
			Images = ["aGVsbG8="]
		}, Offline.Token);

		transport.LastRequest.Json["state"]!.GetValue<string>().Should().StartWith("Our checkout");
		transport.LastRequest.Json["images"]![0]!.GetValue<string>().Should().Be("aGVsbG8=");
		response.Answers["label"].Should().BeOfType<ChoiceAnswer>().Which.Choice.Should().Be("bug");
	}

	/// <summary>
	/// An answer whose type comes after its other fields is still read as its own type.
	/// </summary>
	[Fact]
	public void Answer_TypeAfterOtherFields_IsStillRecognised()
	{
		var response = JsonSerializer.Deserialize<DecisionResponse>(
			"""{"answers":{"a":{"noul":0.25,"type":"noul"}}}""",
			OllamaClient.JsonSerializerOptions)!;

		response.Answers["a"].Should().BeOfType<NoulAnswer>().Which.Noul.Should().Be(0.25);
	}

	/// <summary>
	/// A model that is not a decision model is refused with Ollama's own message.
	/// </summary>
	[Fact]
	public async Task Decide_NotADecisionModel_Throws()
	{
		var transport = new StubTransport().Then(Respond.Fixture(HttpStatusCode.BadRequest, "systemone-not-decision-model.json"));
		using var client = Offline.Client(transport);

		var refusal = await Offline.ShouldBeRefusedAsync(
			() => client.Decisions.DecideAsync(new DecisionRequest
			{
				Model = "qwen3.5:2b",
				State = "x",
				Questions = new() { ["a"] = new NoulQuestion { Instructions = "Is it?" } }
			}, Offline.Token),
			HttpStatusCode.BadRequest);

		refusal.Should().Be("model \"qwen3.5:2b\" is not a llama.cpp decision model");
	}
}
