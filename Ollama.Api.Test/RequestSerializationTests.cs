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
	/// A streamed chat sends every setting of the caller's request, with streaming turned on.
	/// </summary>
	[Fact]
	public void ChatRequest_Streamed_KeepsEverySetting()
	{
		var request = new ChatRequest
		{
			Model = "qwen3.5:2b",
			Messages = [new ChatMessage { Role = "user", Content = "hi" }],
			Stream = false,
			Options = new GenerateOptions { NumPredict = 40 },
			Think = false,
			Format = "json",
			Tools = [],
			KeepAlive = "5m"
		};

		// A setting left null here could be dropped by the copy without this test noticing.
		typeof(ChatRequest).GetProperties().Where(property => property.GetValue(request) is null).Should().BeEmpty();

		var streamed = ChatClient.ToStreamingRequest(request);

		streamed.Stream.Should().BeTrue();
		streamed.Should().BeEquivalentTo(request, options => options.Excluding(setting => setting.Stream));
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
