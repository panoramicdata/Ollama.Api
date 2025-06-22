# Ollama.Api

<img src="Ollama.Api/Logo.png" alt="Ollama.Api Logo" align="right" width="120" />

## Overview
Ollama.Api is a .NET 9 library for interacting with Ollama-compatible AI model APIs. It provides strongly-typed interfaces for chat, text generation, embeddings, and model management, making it easy to integrate advanced AI capabilities into your .NET applications.

## Features
- Chat and text generation endpoints
- Embedding support
- Model management (list, show, copy, delete, push, pull)
- Strongly-typed request/response models
- Async API using Refit
- .NET 9 compatible

## Installation
Add the NuGet package to your project:
```powershell
dotnet add package Ollama.Api
```

## Usage Example

```csharp
using Ollama.Api;
using Ollama.Api.Models;

var client = new OllamaClient(new OllamaClientOptions { BaseUrl = "http://localhost:11434" });
var response = await client.Generate.GenerateAsync(new GenerateRequest {
    Model = "llama3",
    Prompt = "Hello, world!"
}, CancellationToken.None);
Console.WriteLine(response.Response);
```

## License
MIT License. See [LICENSE](LICENSE) for details.

## Copyright
Copyright © Panoramic Data Limited 2025

## Contributing
Contributions are welcome! Please open issues or submit pull requests on GitHub.

## Repository
[https://github.com/panoramicdata/Ollama.Api](https://github.com/panoramicdata/Ollama.Api)