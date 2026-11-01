using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The Ollama server's reported version.
/// </summary>
public class VersionResponse
{
	/// <summary>
	/// The version string, for example <c>0.5.7</c>.
	/// </summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }
}

/// <summary>
/// The models currently loaded into the server's memory.
/// </summary>
public class PsResponse
{
	/// <summary>
	/// The loaded models, or null if the server reported none.
	/// </summary>
	[JsonPropertyName("models")]
	public List<RunningModel>? Models { get; set; }
}

/// <summary>
/// One model held in memory by the server.
/// </summary>
public class RunningModel
{
	/// <summary>
	/// The model's name, including its tag.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>
	/// The model's name as requests should give it.
	/// </summary>
	[JsonPropertyName("model")]
	public string? Model { get; set; }

	/// <summary>
	/// The model's size in memory, in bytes.
	/// </summary>
	[JsonPropertyName("size")]
	public long? Size { get; set; }

	/// <summary>
	/// How many of those bytes are in GPU memory. Less than <see cref="Size"/> means part of the model
	/// runs on the CPU.
	/// </summary>
	[JsonPropertyName("size_vram")]
	public long? SizeVram { get; set; }

	/// <summary>
	/// The SHA-256 digest of the model's manifest.
	/// </summary>
	[JsonPropertyName("digest")]
	public string? Digest { get; set; }

	/// <summary>
	/// The model's format, family, size and quantisation.
	/// </summary>
	[JsonPropertyName("details")]
	public ModelDetails? Details { get; set; }

	/// <summary>
	/// When the model will be unloaded if it is not used again.
	/// </summary>
	[JsonPropertyName("expires_at")]
	public DateTimeOffset? ExpiresAt { get; set; }

	/// <summary>
	/// The context length the model was loaded with.
	/// </summary>
	[JsonPropertyName("context_length")]
	public int? ContextLength { get; set; }

	/// <summary>
	/// The engine running the model, such as <c>llamacpp</c> or <c>mlx</c>.
	/// </summary>
	[JsonPropertyName("runner")]
	public string? Runner { get; set; }

	/// <summary>
	/// What the server reports the model is doing.
	/// </summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>
	/// When the model was loaded into memory.
	/// </summary>
	[JsonPropertyName("loaded_at")]
	public DateTimeOffset? LoadedAt { get; set; }
}
