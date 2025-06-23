using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to pull (download) a model from the Ollama library.
/// </summary>
public class PullModelRequest
{
	/// <summary>
	/// The full name of the model to pull, including its tag (e.g., "llama3.1:latest").
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// If true, allows insecure connections to the model library. Use with caution and only if you are pulling from a trusted, private registry during development. Defaults to false.
	/// </summary>
	[JsonPropertyName("insecure")]
	public bool? Insecure { get; set; }

	/// <summary>
	/// If true (default), the response will be streamed as progress updates. If false, the response will be a single JSON object returned once the download is complete. Defaults to true.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool? Stream { get; set; }

	/// <summary>
	/// The model digest to pull. Used to ensure a specific version of a model is pulled.
	/// </summary>
	[JsonPropertyName("digest")]
	public string? Digest { get; set; }
}

