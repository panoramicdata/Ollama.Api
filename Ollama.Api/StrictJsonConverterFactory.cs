using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ollama.Api;

public class StrictJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		// This factory will handle all types for deserialization
		return true;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		// Create an instance of StrictJsonConverter<T> for the specific type
		var converterType = typeof(StrictJsonConverter<>).MakeGenericType(typeToConvert);
		return (JsonConverter)Activator.CreateInstance(converterType);
	}
}