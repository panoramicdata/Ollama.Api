using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A request to <c>/api/embed</c> for one embedding per input text.
/// </summary>
public class EmbedRequest
{
	/// <summary>
	/// The embedding model, such as <c>nomic-embed-text</c>.
	/// </summary>
	[JsonPropertyName("model")]
	public required string Model { get; set; }

	/// <summary>
	/// The texts to embed. The response holds one vector per text, in the same order.
	/// </summary>
	[JsonPropertyName("input")]
	public required IReadOnlyList<string> Input { get; set; }

	/// <summary>
	/// Whether to truncate an input longer than the context window rather than fail. Defaults to true on the server.
	/// </summary>
	[JsonPropertyName("truncate")]
	public bool? Truncate { get; set; }

	/// <summary>
	/// How many dimensions each vector should have, for a model that supports shortening them.
	/// </summary>
	[JsonPropertyName("dimensions")]
	public int? Dimensions { get; set; }

	/// <summary>
	/// How long the model stays loaded after the request, such as <c>5m</c>.
	/// </summary>
	[JsonPropertyName("keep_alive")]
	public string? KeepAlive { get; set; }

	/// <summary>
	/// Runtime options for the model.
	/// </summary>
	[JsonPropertyName("options")]
	public GenerateOptions? Options { get; set; }
}
