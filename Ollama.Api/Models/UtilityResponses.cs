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
