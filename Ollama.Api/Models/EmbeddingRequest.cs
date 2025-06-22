using Ollama.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Request for the embeddings API.
/// </summary>
public class EmbeddingRequest : IValidated
{
	/// <summary>
	/// The name of the model to use for generating the embeddings (e.g. "llama3.1" or "nomic-embed-text").
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// The single text string for which an embedding should be generated. Use this for individual texts. If input is provided, prompt is ignored.
	/// </summary>
	[JsonPropertyName("prompt")]
	public string? Prompt { get; set; }

	/// <summary>
	/// An array of text strings for which embeddings should be generated. If input is provided, prompt is ignored.
	/// </summary>
	[JsonPropertyName("input")]
	public IEnumerable<string>? Input { get; set; }

	/// <summary>
	/// If true (default), the end of each input will be truncated to fit within the context length of the model. If false, inputs exceeding the context length will result in an error. Defaults to true.
	/// </summary>
	[JsonPropertyName("truncate")]
	public bool? Truncate { get; set; }

	/// <summary>
	/// An object containing advanced model parameters. While embedding models have fewer relevant options, some (like num_thread) can still apply.
	/// </summary>
	[JsonPropertyName("options")]
	public GenerateOptions? Options { get; set; }

	/// <summary>
	/// Controls how long the model will stay loaded into memory following the request. Can be a duration string (e.g., "5m", "10s") or an integer representing seconds. Defaults to "5m" (5 minutes).
	/// </summary>
	[JsonPropertyName("keep_alive")]
	public string? KeepAlive { get; set; }

	public void Validate()
	{
		// Either prompt or input must be provided, but not both or neither
		if ((Prompt is null && Input is null) || (Prompt is not null && Input is not null))
		{
			throw new ArgumentException("Either 'prompt' or 'input' must be provided, but not both.");
		}
	}
}
