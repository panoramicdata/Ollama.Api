using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

public class VersionResponse
{
	[JsonPropertyName("version")]
	public string? Version { get; set; }
}

public class PsResponse
{
	[JsonPropertyName("models")]
	public List<RunningModel>? Models { get; set; }
}

public class RunningModel
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }
	[JsonPropertyName("status")]
	public string? Status { get; set; }
	[JsonPropertyName("loaded_at")]
	public DateTimeOffset? LoadedAt { get; set; }
}
