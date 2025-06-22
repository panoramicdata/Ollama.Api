using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// Options for controlling the text generation process and model behavior.
/// </summary>
public class GenerateOptions
{
	/// <summary>
	/// Enable Mirostat sampling. Controls the perplexity of the output. 0 for disabled, 1 for Mirostat, 2 for Mirostat 2.
	/// </summary>
	[JsonPropertyName("mirostat")]
	public int? Mirostat { get; set; }

	/// <summary>
	/// Influences the coherence and quality of the output in Mirostat sampling.
	/// </summary>
	[JsonPropertyName("mirostat_eta")]
	public double? MirostatEta { get; set; }

	/// <summary>
	/// Controls the target perplexity in Mirostat sampling.
	/// </summary>
	[JsonPropertyName("mirostat_tau")]
	public double? MirostatTau { get; set; }

	/// <summary>
	/// Sets the size of the context window in tokens.
	/// </summary>
	[JsonPropertyName("num_ctx")]
	public int? NumCtx { get; set; }

	/// <summary>
	/// Number of GQA groups in the transformer architecture.
	/// </summary>
	[JsonPropertyName("num_gqa")]
	public int? NumGqa { get; set; }

	/// <summary>
	/// The number of layers to offload to the GPU. Set to 0 to disable GPU, -1 for all layers.
	/// </summary>
	[JsonPropertyName("num_gpu")]
	public int? NumGpu { get; set; }

	/// <summary>
	/// Sets the number of threads to use during computation. 0 allows Ollama to detect for optimal performance (recommended).
	/// </summary>
	[JsonPropertyName("num_thread")]
	public int? NumThread { get; set; }

	/// <summary>
	/// Sets how far back the model looks to prevent repetition. 0 disables, -1 uses num_ctx.
	/// </summary>
	[JsonPropertyName("repeat_last_n")]
	public int? RepeatLastN { get; set; }

	/// <summary>
	/// Penalizes repeating tokens. Higher values make the model less likely to repeat itself.
	/// </summary>
	[JsonPropertyName("repeat_penalty")]
	public double? RepeatPenalty { get; set; }

	/// <summary>
	/// The temperature of the model. Increasing the temperature will make the model's responses more creative and random; lower values make them more deterministic.
	/// </summary>
	[JsonPropertyName("temperature")]
	public double? Temperature { get; set; }

	/// <summary>
	/// Sets the random number seed to use for generation. Setting this to a specific number makes the model generate the same text for the same prompt, ensuring reproducible outputs.
	/// </summary>
	[JsonPropertyName("seed")]
	public int? Seed { get; set; }

	/// <summary>
	/// A list of strings that, if encountered, will stop the model from generating further tokens.
	/// </summary>
	[JsonPropertyName("stop")]
	public List<string>? Stop { get; set; }

	/// <summary>
	/// Tail-free sampling. Reduces the impact of less probable tokens from the output. 1.0 disables.
	/// </summary>
	[JsonPropertyName("tfs_z")]
	public double? TfsZ { get; set; }

	/// <summary>
	/// Maximum number of tokens to predict. -1 allows infinite generation, -2 fills the context.
	/// </summary>
	[JsonPropertyName("num_predict")]
	public int? NumPredict { get; set; }

	/// <summary>
	/// Reduces the probability of generating nonsense. A higher value (e.g., 100) will give more diverse answers, while a lower value (e.g., 10) will be more conservative.
	/// </summary>
	[JsonPropertyName("top_k")]
	public int? TopK { get; set; }

	/// <summary>
	/// Works together with top_k. A higher value (e.g., 0.95) will lead to more diverse text, while a lower value (e.g., 0.5) will generate more focused and conservative text.
	/// </summary>
	[JsonPropertyName("top_p")]
	public double? TopP { get; set; }

	/// <summary>
	/// Sets the prompt processing maximum batch size.
	/// </summary>
	[JsonPropertyName("num_batch")]
	public int? NumBatch { get; set; }

	/// <summary>
	/// Use float16 for key/value cache.
	/// </summary>
	[JsonPropertyName("f16_kv")]
	public bool? F16Kv { get; set; }

	/// <summary>
	/// Reduce VRAM usage at the cost of speed.
	/// </summary>
	[JsonPropertyName("low_vram")]
	public bool? LowVram { get; set; }

	/// <summary>
	/// When using multiple GPUs, controls which GPU is used for small tensors.
	/// </summary>
	[JsonPropertyName("main_gpu")]
	public int? MainGpu { get; set; }

	/// <summary>
	/// Number of tokens to keep from the original prompt for the next generation.
	/// </summary>
	[JsonPropertyName("num_keep")]
	public int? NumKeep { get; set; }

	/// <summary>
	/// Path to a LoRA adapter to apply to the model.
	/// </summary>
	[JsonPropertyName("lora")]
	public string? Lora { get; set; }

	/// <summary>
	/// Disable Q4_0 and Q4_1 quantization for matrix multiplication.
	/// </summary>
	[JsonPropertyName("no_mul_mat_q")]
	public bool? NoMulMatQ { get; set; }

	/// <summary>
	/// Enable NUMA support.
	/// </summary>
	[JsonPropertyName("numa")]
	public bool? Numa { get; set; }

	/// <summary>
	/// Sets the RoPE frequency base.
	/// </summary>
	[JsonPropertyName("rope_frequency_base")]
	public double? RopeFrequencyBase { get; set; }

	/// <summary>
	/// Sets the RoPE frequency scale.
	/// </summary>
	[JsonPropertyName("rope_frequency_scale")]
	public double? RopeFrequencyScale { get; set; }

	/// <summary>
	/// Batch size for prompt evaluation.
	/// </summary>
	[JsonPropertyName("eval_batch_size")]
	public int? EvalBatchSize { get; set; }

	/// <summary>
	/// The GQA value.
	/// </summary>
	[JsonPropertyName("gqa")]
	public int? Gqa { get; set; }

	/// <summary>
	/// Not commonly used directly in API.
	/// </summary>
	[JsonPropertyName("idx")]
	public int? Idx { get; set; }

	/// <summary>
	/// Epsilon for group normalization.
	/// </summary>
	[JsonPropertyName("group_norm_eps")]
	public double? GroupNormEps { get; set; }

	/// <summary>
	/// Logits regularization.
	/// </summary>
	[JsonPropertyName("log_reg_norm")]
	public double? LogRegNorm { get; set; }

	/// <summary>
	/// Maximum number of tokens to generate. A more user-friendly alternative to num_predict.
	/// </summary>
	[JsonPropertyName("max_tokens")]
	public int? MaxTokens { get; set; }

	/// <summary>
	/// Alternative to top_p. A parameter p represents the minimum probability for a token to be considered, relative to the probability of the most likely token.
	/// </summary>
	[JsonPropertyName("min_p")]
	public double? MinP { get; set; }

	/// <summary>
	/// Whether to penalize newline tokens.
	/// </summary>
	[JsonPropertyName("penalize_newline")]
	public bool? PenalizeNewline { get; set; }

	/// <summary>
	/// Penalizes new tokens based on whether they appear in the text so far.
	/// </summary>
	[JsonPropertyName("presence_penalty")]
	public double? PresencePenalty { get; set; }

	/// <summary>
	/// Penalizes new tokens based on their frequency in the text so far.
	/// </summary>
	[JsonPropertyName("frequency_penalty")]
	public double? FrequencyPenalty { get; set; }

	/// <summary>
	/// Whether to process the prompt.
	/// </summary>
	[JsonPropertyName("process_prompt")]
	public bool? ProcessPrompt { get; set; }

	/// <summary>
	/// A parameter that aims to ensure a balance of quality and variety, often used as an alternative to top_p.
	/// </summary>
	[JsonPropertyName("typical_p")]
	public double? TypicalP { get; set; }

	/// <summary>
	/// By default, models are memory-mapped. true uses memory mapping; false loads the entire model into RAM.
	/// </summary>
	[JsonPropertyName("use_mmap")]
	public bool? UseMmap { get; set; }

	/// <summary>
	/// Lock the model in memory, preventing it from being swapped out. Can improve performance but uses more RAM.
	/// </summary>
	[JsonPropertyName("use_mlock")]
	public bool? UseMlock { get; set; }

	/// <summary>
	/// Load only the vocabulary, not the weights.
	/// </summary>
	[JsonPropertyName("vocab_only")]
	public bool? VocabOnly { get; set; }
}
