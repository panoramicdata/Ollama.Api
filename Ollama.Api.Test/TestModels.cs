namespace Ollama.Api.Test;

/// <summary>
/// Enumeration of models used in unit tests.
/// </summary>
public enum ModelType
{
	/// <summary>
	/// Llama 3 latest version - General purpose language model
	/// </summary>
	Llama3Latest,

	/// <summary>
	/// Llama 3 base version - General purpose language model
	/// </summary>
	Llama3,

	/// <summary>
	/// Llama 3.1 - General purpose language model with tool calling support
	/// </summary>
	Llama31,

	/// <summary>
	/// Nomic Embed Text - Specialized model for text embeddings
	/// </summary>
	NomicEmbedText,

	/// <summary>
	/// Llava latest version - Multimodal model supporting images
	/// </summary>
	LlavaLatest
}

/// <summary>
/// Static configuration for test models.
/// </summary>
public static class TestModels
{
	/// <summary>
	/// Maps model types to their Ollama model names.
	/// </summary>
	public static readonly IReadOnlyDictionary<ModelType, string> Models = new Dictionary<ModelType, string>
	{
		{ ModelType.Llama3Latest, "llama3:latest" },
		{ ModelType.Llama3, "llama3" },
		{ ModelType.Llama31, "llama3.1" },
		{ ModelType.NomicEmbedText, "nomic-embed-text" },
		{ ModelType.LlavaLatest, "llava:latest" }
	};

	/// <summary>
	/// Gets the model name for the specified model type.
	/// </summary>
	/// <param name="modelType">The model type.</param>
	/// <returns>The Ollama model name.</returns>
	public static string GetModelName(ModelType modelType) => Models[modelType];

	/// <summary>
	/// Gets all model names that should be downloaded for running tests.
	/// </summary>
	/// <returns>An enumerable of model names.</returns>
	public static IEnumerable<string> GetAllModelNames() => Models.Values;
}
