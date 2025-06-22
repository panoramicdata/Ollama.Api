using System.Text.Json.Serialization;
using System.Collections.Generic;

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
    /// The size of the model in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long? Size { get; set; }

    /// <summary>
    /// The last modified timestamp of the model.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTimeOffset? ModifiedAt { get; set; }
}
