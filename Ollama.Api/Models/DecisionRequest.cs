using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A request to a decision model through <c>/v1/systemone</c>: typed questions about one shared state.
/// </summary>
public class DecisionRequest
{
	/// <summary>
	/// A decision model, such as <c>nimble</c>, <c>d3</c> or <c>clef:cloud</c>.
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// What the questions are about: a non-blank string, or any object or array, which is sent as JSON.
	/// </summary>
	[JsonPropertyName("state")]
	public required object State { get; set; }

	/// <summary>
	/// The questions, keyed by the names their answers come back under.
	/// </summary>
	[JsonPropertyName("questions")]
	public required Dictionary<string, DecisionQuestion> Questions { get; set; }

	/// <summary>
	/// Base64-encoded images shared by every question, for a model with vision. Audio models also
	/// accept WAV clips here.
	/// </summary>
	[JsonPropertyName("images")]
	public List<string>? Images { get; set; }

	/// <summary>
	/// How long the model stays loaded after the request, such as <c>5m</c>.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	public string? KeepAlive { get; set; }
}

/// <summary>
/// One question for a decision model.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChoiceQuestion), "choice")]
[JsonDerivedType(typeof(NoulQuestion), "noul")]
[JsonDerivedType(typeof(ScoreQuestion), "score")]
public abstract class DecisionQuestion
{
	/// <summary>
	/// The question: a non-blank string, or any object or array, which is sent as JSON.
	/// </summary>
	[JsonPropertyName("instructions")]
	public required object Instructions { get; set; }
}

/// <summary>
/// Choose one of several labelled options.
/// </summary>
public class ChoiceQuestion : DecisionQuestion
{
	/// <summary>
	/// The option keys, each with a description or null for a bare label. At least two.
	/// </summary>
	[JsonPropertyName("criteria")]
	public required Dictionary<string, string?> Criteria { get; set; }
}

/// <summary>
/// Answer yes or no, as the probability of yes.
/// </summary>
public class NoulQuestion : DecisionQuestion
{
	/// <summary>
	/// Optional descriptions of the two outcomes.
	/// </summary>
	[JsonPropertyName("criteria")]
	public NoulCriteria? Criteria { get; set; }
}

/// <summary>
/// The descriptions of a yes/no question's two outcomes. An omitted one takes the model's default.
/// </summary>
public class NoulCriteria
{
	/// <summary>
	/// What "no" means.
	/// </summary>
	[JsonPropertyName("false")]
	public string? False { get; set; }

	/// <summary>
	/// What "yes" means.
	/// </summary>
	[JsonPropertyName("true")]
	public string? True { get; set; }
}

/// <summary>
/// Score on an ordered scale.
/// </summary>
public class ScoreQuestion : DecisionQuestion
{
	/// <summary>
	/// The levels, lowest first: between two and ten. The score runs from 0 to one less than the count.
	/// </summary>
	[JsonPropertyName("criteria")]
	public required List<string> Criteria { get; set; }
}
