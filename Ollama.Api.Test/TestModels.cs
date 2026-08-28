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
	/// Nomic Embed Text - Specialized model for text embeddings
	/// </summary>
	NomicEmbedText,

	/// <summary>
	/// Llava latest version - Multimodal model supporting images
	/// </summary>
	LlavaLatest,

	/// <summary>
	/// Qwen 3.5 2b - Specialized model for code generation
	/// </summary>
	Qwen352b,

	/// <summary>
	/// Gemma 4 E2b Large language model with enhanced reasoning capabilities
	/// </summary>
	Gemma4E2b,

	/// <summary>
	/// Gemma 4 E4b Large language model with enhanced reasoning capabilities
	/// </summary>
	Gemma4E4b
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
		{ ModelType.NomicEmbedText, "nomic-embed-text" },
		{ ModelType.LlavaLatest, "llava:latest" },
		{ ModelType.Qwen352b, "qwen3.5:2b" },
		{ ModelType.Gemma4E2b, "gemma4:e2b" },
		{ ModelType.Gemma4E4b, "gemma4:e4b" },
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
