using AwesomeAssertions;
using Ollama.Api.Models;
using System.Text.Json;

namespace Ollama.Api.Test;

/// <summary>
/// Where request settings land in the JSON sent to Ollama, which needs no Ollama server.
/// </summary>
public class RequestSerializationTests
{
	/// <summary>
	/// Ollama reads think only at the top level of a chat request.
	/// </summary>
	[Fact]
	public void ChatRequest_Think_IsSentAtTheTopLevel()
	{
		var json = JsonSerializer.SerializeToNode(
			new ChatRequest { Model = "qwen3.5:2b", Messages = [], Think = false },
			OllamaClient.JsonSerializerOptions)!;

		json["think"]!.GetValue<bool>().Should().BeFalse();
	}

	/// <summary>
	/// Ollama reads think only at the top level of a generate request.
	/// </summary>
	[Fact]
	public void GenerateRequest_Think_IsSentAtTheTopLevel()
	{
		var json = JsonSerializer.SerializeToNode(
			new GenerateRequest { Model = "qwen3.5:2b", Prompt = "hi", Think = false },
			OllamaClient.JsonSerializerOptions)!;

		json["think"]!.GetValue<bool>().Should().BeFalse();
	}
}
