using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Represents the response from the /api/tags endpoint (list of local models).
/// </summary>
public class TagsResponse
{
	/// <summary>
	/// The list of local models available in Ollama storage.
	/// </summary>
	[JsonPropertyName("models")]
	public List<ModelTag>? Models { get; set; }
}

/// <summary>
/// Represents a model tag entry in the tags response.
/// </summary>
public class ModelTag
{
	/// <summary>
	/// The name of the model.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// The model's name as requests should give it.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// The size of the model in bytes.
	/// </summary>
	[JsonPropertyName("size")]
	public long? Size { get; set; }

	/// <summary>
	/// The SHA-256 digest of the model's manifest.
	/// </summary>
	[JsonPropertyName("digest")]
	public string? Digest { get; set; }

	/// <summary>
	/// The last modified timestamp of the model.
	/// </summary>
	[JsonPropertyName("modified_at")]
	public DateTimeOffset? ModifiedAt { get; set; }

	/// <summary>
	/// The model's format, family, size and quantisation.
	/// </summary>
	[JsonPropertyName("details")]
	public ModelDetails? Details { get; set; }

	/// <summary>
	/// The upstream model's name, for a model that runs on another Ollama host.
	/// </summary>
	[JsonPropertyName("remote_model")]
	public string? RemoteModel { get; set; }

	/// <summary>
	/// The upstream Ollama host, for a model that runs remotely.
	/// </summary>
	[JsonPropertyName("remote_host")]
	public string? RemoteHost { get; set; }
}
