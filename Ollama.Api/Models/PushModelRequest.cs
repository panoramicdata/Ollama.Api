using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents a request to push (upload) a custom local model to a model registry.
/// </summary>
public class PushModelRequest
{
	/// <summary>
	/// The full name of the model to push, including its tag and namespace (e.g., "myuser/mymodel:latest").
	/// </summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>
	/// If true, allows insecure connections to the model library. Use with caution and only if you are pushing to a trusted, private registry during development. Defaults to false.
	/// </summary>
	[JsonPropertyName("insecure")]
	public bool? Insecure { get; set; }

	/// <summary>
	/// If true (default), the response will be streamed as progress updates. If false, the response will be a single JSON object returned once the upload is complete. Defaults to true.
	/// </summary>
	[JsonPropertyName("stream")]
	public bool? Stream { get; set; }
}
