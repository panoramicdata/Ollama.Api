using System.Text.Json.Serialization;

namespace Ollama.Api.Models;

/// <summary>
/// The log probability of one candidate token.
/// </summary>
public class TokenLogprob
{
	/// <summary>
	/// The token's text.
	/// </summary>
	[JsonPropertyName("token")]
	public string? Token { get; set; }

	/// <summary>
	/// The natural log of the token's probability.
	/// </summary>
	[JsonPropertyName("logprob")]
	public double Logprob { get; set; }

	/// <summary>
	/// The token's raw bytes, which may be part of a multi-byte character.
	/// </summary>
	[JsonPropertyName("bytes")]
	public List<int>? Bytes { get; set; }
}

/// <summary>
/// The log probability of a generated token, with the likeliest alternatives at its position.
/// </summary>
public class Logprob : TokenLogprob
{
	/// <summary>
	/// The likeliest tokens at this position, when the request set <c>top_logprobs</c>.
	/// </summary>
	[JsonPropertyName("top_logprobs")]
	public List<TokenLogprob>? TopLogprobs { get; set; }
}
