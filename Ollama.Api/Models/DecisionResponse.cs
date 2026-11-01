using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A decision model's answers to a <see cref="DecisionRequest"/>.
/// </summary>
public class DecisionResponse
{
	/// <summary>
	/// The model named in the request.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// The answers, keyed by the question names in the request.
	/// </summary>
	[JsonPropertyName("answers")]
	public Dictionary<string, DecisionAnswer> Answers { get; set; } = [];

	/// <summary>
	/// The tokens the request used.
	/// </summary>
	[JsonPropertyName("usage")]
	public DecisionUsage? Usage { get; set; }
}

/// <summary>
/// The answer to one question.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChoiceAnswer), "choice")]
[JsonDerivedType(typeof(NoulAnswer), "noul")]
[JsonDerivedType(typeof(ScoreAnswer), "score")]
public abstract class DecisionAnswer;

/// <summary>
/// The answer to a <see cref="ChoiceQuestion"/>.
/// </summary>
public class ChoiceAnswer : DecisionAnswer
{
	/// <summary>
	/// The likeliest option's key.
	/// </summary>
	[JsonPropertyName("choice")]
	public string? Choice { get; set; }

	/// <summary>
	/// Each option's probability, summing to 1.
	/// </summary>
	[JsonPropertyName("probabilities")]
	public Dictionary<string, double> Probabilities { get; set; } = [];

	/// <summary>
	/// How strongly one option is favoured, from 0 (all equal) to 1. Not a measure of correctness.
	/// </summary>
	[JsonPropertyName("confidence")]
	public double Confidence { get; set; }
}

/// <summary>
/// The answer to a <see cref="NoulQuestion"/>.
/// </summary>
public class NoulAnswer : DecisionAnswer
{
	/// <summary>
	/// The probability of yes, from 0 to 1.
	/// </summary>
	[JsonPropertyName("noul")]
	public double Noul { get; set; }
}

/// <summary>
/// The answer to a <see cref="ScoreQuestion"/>.
/// </summary>
public class ScoreAnswer : DecisionAnswer
{
	/// <summary>
	/// The probability-weighted average of the zero-based levels. Not rounded to a level.
	/// </summary>
	[JsonPropertyName("score")]
	public double Score { get; set; }

	/// <summary>
	/// Each level's description, keyed by its zero-based index as a string.
	/// </summary>
	[JsonPropertyName("legend")]
	public Dictionary<string, string> Legend { get; set; } = [];

	/// <summary>
	/// Each level's probability, keyed by its zero-based index as a string.
	/// </summary>
	[JsonPropertyName("probabilities")]
	public Dictionary<string, double> Probabilities { get; set; } = [];

	/// <summary>
	/// How strongly one level is favoured, from 0 (all equal) to 1. Not a measure of correctness.
	/// </summary>
	[JsonPropertyName("confidence")]
	public double Confidence { get; set; }
}

/// <summary>
/// The tokens a decision request used.
/// </summary>
public class DecisionUsage
{
	/// <summary>
	/// Input tokens evaluated, including image positions and context counted again per question.
	/// </summary>
	[JsonPropertyName("input_tokens")]
	public int InputTokens { get; set; }

	/// <summary>
	/// Tokens generated internally for scoring. Not the length of the response.
	/// </summary>
	[JsonPropertyName("output_tokens")]
	public int OutputTokens { get; set; }
}
