using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Enumerates the supported types for chat tool function input schema properties.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum McpType
{
	/// <summary>
	/// Represents a string type.
	/// </summary>
	[EnumMember(Value = "string")]
	String,

	/// <summary>
	/// Represents an integer type.
	/// </summary>
	[EnumMember(Value = "integer")]
	Integer,

	/// <summary>
	/// Represents a number type.
	/// </summary>
	[EnumMember(Value = "number")]
	Number,

	/// <summary>
	/// Represents a boolean type.
	/// </summary>
	[EnumMember(Value = "boolean")]
	Boolean,

	/// <summary>
	/// Represents an object type.
	/// </summary>
	[EnumMember(Value = "object")]
	Object,

	/// <summary>
	/// Represents an array type.
	/// </summary>
	[EnumMember(Value = "array")]
	Array,

	/// <summary>
	/// Represents a function.
	/// </summary>
	[EnumMember(Value = "function")]
	Function
}