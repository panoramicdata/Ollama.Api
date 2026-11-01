using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// A thinking setting: on or off, or a level a model names itself, such as <c>low</c> or <c>high</c>.
/// </summary>
/// <remarks>
/// <c>/api/show</c> lists the values a model accepts in <see cref="ShowModelResponse.Thinking"/>.
/// </remarks>
[JsonConverter(typeof(ThinkValueJsonConverter))]
public readonly record struct ThinkValue
{
	private ThinkValue(bool? enabled, string? level)
	{
		Enabled = enabled;
		Level = level;
	}

	/// <summary>
	/// The on/off value, or null when this is a named level.
	/// </summary>
	public bool? Enabled { get; }

	/// <summary>
	/// The named level, or null when this is an on/off value.
	/// </summary>
	public string? Level { get; }

	/// <summary>
	/// An on/off value.
	/// </summary>
	public static ThinkValue FromBoolean(bool enabled) => new(enabled, null);

	/// <summary>
	/// A named level.
	/// </summary>
	/// <exception cref="ArgumentException"><paramref name="level"/> is null or blank.</exception>
	public static ThinkValue FromLevel(string level)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(level);
		return new(null, level);
	}

	/// <summary>
	/// An on/off value.
	/// </summary>
	public static implicit operator ThinkValue(bool enabled) => FromBoolean(enabled);

	/// <summary>
	/// A named level.
	/// </summary>
	public static implicit operator ThinkValue(string level) => FromLevel(level);

	/// <inheritdoc />
	public override string ToString() => Level ?? (Enabled == true ? "true" : "false");
}

/// <summary>
/// Reads and writes a <see cref="ThinkValue"/> as the JSON boolean or string Ollama uses.
/// </summary>
internal sealed class ThinkValueJsonConverter : JsonConverter<ThinkValue>
{
	public override ThinkValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> reader.TokenType switch
		{
			JsonTokenType.True => ThinkValue.FromBoolean(true),
			JsonTokenType.False => ThinkValue.FromBoolean(false),
			JsonTokenType.String => ThinkValue.FromLevel(reader.GetString()!),
			_ => throw new JsonException($"A think value must be a boolean or a string, not {reader.TokenType}.")
		};

	public override void Write(Utf8JsonWriter writer, ThinkValue value, JsonSerializerOptions options)
	{
		if (value.Level is not null)
		{
			writer.WriteStringValue(value.Level);
			return;
		}

		writer.WriteBooleanValue(value.Enabled == true);
	}
}
