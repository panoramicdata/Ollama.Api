using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ollama.Api;

public class StrictJsonConverter<T> : JsonConverter<T>
{
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Make a copy of the reader to allow peeking at the original JSON
		var originalReader = reader;
		using var jsonDocument = JsonDocument.ParseValue(ref reader);
		var rootElement = jsonDocument.RootElement;

		// Deserialize the object using the default behavior (ignoring extra properties)
		// We pass 'null' for options here to avoid infinite recursion if this converter
		// is added directly to 'options.Converters'. Instead, we rely on the internal
		// default deserialization logic for the type.
		// Or, better, create a new JsonSerializerOptions instance without this converter
		// to avoid infinite recursion.
		var newOptions = new JsonSerializerOptions(options);
		newOptions.Converters.Remove(this); // Remove this converter to prevent infinite loop

		var deserializedObject = (T)JsonSerializer.Deserialize(rootElement.GetRawText(), typeToConvert, newOptions);

		if (deserializedObject is null)
		{
			return default;
		}

		// Get properties of the C# model
		var modelProperties = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => p.CanWrite) // Only consider properties that can be set
			.ToDictionary(p => StrictJsonConverter<T>.GetJsonPropertyName(p, options), p => p, StringComparer.OrdinalIgnoreCase);

		// Check for extra properties in the JSON
		foreach (var jsonProperty in rootElement.EnumerateObject())
		{
			// Use the property naming policy to get the expected C# property name
			// from the JSON property name.
			var expectedPropertyName = options.PropertyNamingPolicy?.ConvertName(jsonProperty.Name) ?? jsonProperty.Name;

			if (!modelProperties.ContainsKey(expectedPropertyName))
			{
				throw new JsonException($"JSON contains an unexpected property: '{jsonProperty.Name}' for type '{typeToConvert.FullName}'.");
			}
		}

		return deserializedObject;
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		// For serialization, we can just use the default behavior.
		// Again, create new options to avoid infinite recursion.
		var newOptions = new JsonSerializerOptions(options);
		newOptions.Converters.Remove(this);
		JsonSerializer.Serialize(writer, value, newOptions);
	}

	// Helper to get the actual JSON property name considering JsonPropertyName attribute or naming policy
	private static string GetJsonPropertyName(PropertyInfo propertyInfo, JsonSerializerOptions options)
	{
		var jsonPropertyNameAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
		if (jsonPropertyNameAttribute != null)
		{
			return jsonPropertyNameAttribute.Name;
		}

		return options.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
	}
}
