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

## Setting Up a Local Ollama Server on Windows

If you have Ollama installed on Windows, you can run a local server on port 11434 (the default port).

### Running Ollama Server

1. **Start the Ollama service** (if not already running):
   ```powershell
   ollama serve
   ```
   This will start the Ollama server on `http://localhost:11434`.

2. **Verify the server is running**:
   Open a browser and navigate to `http://localhost:11434`. You should see a response indicating the server is running.

3. **Pull a model** (if you haven't already):
   ```powershell
   ollama pull llama3
   ```

4. **Test the model** (optional):
   ```powershell
   ollama run llama3 "Hello, world!"
   ```

### Downloading Models Required for Unit Tests

If you plan to run the unit tests for this library, you'll need to download the following models:

```powershell
# General purpose language models
ollama pull llama3:latest
ollama pull llama3
ollama pull llama3.1

# Embedding model
ollama pull nomic-embed-text

# Multimodal model (supports images)
ollama pull llava:latest
```

You can verify which models are installed using:
```powershell
ollama list
```

### Configuring Ollama to Run on a Different Port

If you need to run Ollama on a different port, set the `OLLAMA_HOST` environment variable before starting the server:

```powershell
$env:OLLAMA_HOST = "0.0.0.0:11434"
ollama serve
```

### Running Ollama as a Windows Service

Ollama doesn't install as a Windows service by default, but you can configure it to start automatically at boot using Windows Task Scheduler:

1. **Create a scheduled task to run Ollama at startup** (run PowerShell as Administrator):
   ```powershell
   $action = New-ScheduledTaskAction -Execute "C:\Users\$env:USERNAME\AppData\Local\Programs\Ollama\ollama.exe" -Argument "serve"
   $trigger = New-ScheduledTaskTrigger -AtStartup
   $principal = New-ScheduledTaskPrincipal -UserId "$env:USERNAME" -LogonType S4U -RunLevel Highest
   $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit 0
   Register-ScheduledTask -TaskName "Ollama" -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description "Ollama language model service"
   ```

2. **Start the task immediately**:
   ```powershell
   Start-ScheduledTask -TaskName "Ollama"
   ```

3. **Verify Ollama is running**:
   ```powershell
   Get-Process ollama
   ```

4. **Stop the task**:
   ```powershell
   Stop-ScheduledTask -TaskName "Ollama"
   ```

5. **Remove the scheduled task** (if needed):
   ```powershell
   Unregister-ScheduledTask -TaskName "Ollama" -Confirm:$false
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
Copyright � Panoramic Data Limited 2025

## Contributing
Contributions are welcome! Please open issues or submit pull requests on GitHub.

## Repository
[https://github.com/panoramicdata/Ollama.Api](https://github.com/panoramicdata/Ollama.Api)