using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The thinking controls a model advertises through <c>/api/show</c>.
/// </summary>
public class ThinkingInfo
{
	/// <summary>
	/// The values the model accepts for <c>think</c>. Only <c>false</c> means the model cannot think.
	/// </summary>
	[JsonPropertyName("values")]
	public List<ThinkValue> Values { get; set; } = [];

	/// <summary>
	/// The value used when a request does not set <c>think</c>.
	/// </summary>
	[JsonPropertyName("default")]
	public ThinkValue Default { get; set; }
}
