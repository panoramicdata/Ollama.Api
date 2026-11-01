using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The settings chat and generate requests share.
/// </summary>
public abstract class InferenceRequest
{
	/// <summary>
	/// The model to run, which must be available on the server (e.g., "llama3.1").
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// Model parameters that override the Modelfile's defaults for this request.
	/// </summary>
	[JsonPropertyName("options")]
	public GenerateOptions? Options { get; set; }

	/// <summary>
	/// Whether a model that can reason does so before it answers: true, false, or a level the model
	/// names, such as "high". Null leaves the model's default.
	/// </summary>
	[JsonPropertyName("think")]
	public ThinkValue? Think { get; set; }

	/// <summary>
	/// Whether to return the log probability of each generated token.
	/// </summary>
	[JsonPropertyName("logprobs")]
	public bool? Logprobs { get; set; }

	/// <summary>
	/// How many of the likeliest alternatives to return at each position, when <see cref="Logprobs"/> is set.
	/// </summary>
	[JsonPropertyName("top_logprobs")]
	public int? TopLogprobs { get; set; }

	/// <summary>
	/// The output format: the string "json", or a JSON schema object the output must match.
	/// </summary>
	[JsonPropertyName("format")]
	public object? Format { get; set; }

	/// <summary>
	/// How long the model stays loaded after the request, such as "5m". Defaults to five minutes.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	public string? KeepAlive { get; set; }
}
