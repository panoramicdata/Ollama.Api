# Ollama.Api

[![Nuget](https://img.shields.io/nuget/v/Ollama.Api)](https://www.nuget.org/packages/Ollama.Api/)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/eaba65b3e3c54445b26507f317d4cbeb)](https://app.codacy.com/gh/panoramicdata/Ollama.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

<img src="Ollama.Api/Logo.png" alt="Ollama.Api Logo" align="right" width="120" />

## Overview
Ollama.Api is a .NET 10 library for interacting with Ollama-compatible AI model APIs. It provides strongly-typed interfaces for chat, text generation, embeddings, speech, decision models and model management, making it easy to integrate advanced AI capabilities into your .NET applications.

Covers the native API and the audio and decision endpoints of Ollama 0.40.13.

## Features
- Chat (buffered and streamed) and text generation, with thinking levels, structured output by JSON schema, and log probabilities
- Embeddings through `/api/embed`, with dimensions, and the older `/api/embeddings`
- Speech: text to speech (`/v1/audio/speech`), streamed as it is generated, plus transcription and translation
- Decision models (`/v1/systemone`): choice, yes/no and score questions with probabilities
- Model management: list, show (capabilities, thinking values, voices), copy, delete, pull, push, create, and blobs
- Server status (version, loaded models) and Ollama cloud usage and balance
- Strongly-typed request/response models
- Async API using Refit
- .NET 10 compatible

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

The tests in `Ollama.Api.Test/Offline` need no server. The others run against the server named in
user secrets (see `usersecrets.example.json`), which must be Ollama 0.40.13 or later and hold the
models in `TestModels.cs`:

```powershell
# General purpose and thinking language models
ollama pull llama3:latest
ollama pull qwen3.5:2b
ollama pull gemma4:e2b
ollama pull gemma4:e4b

# Embedding model
ollama pull nomic-embed-text

# Multimodal model (supports images)
ollama pull llava:latest

# Speech model (text to speech and transcription) and decision model
ollama pull lfm3-audio
ollama pull d3:2b
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

using var client = new OllamaClient(new OllamaClientOptions { Uri = new Uri("http://localhost:11434") });
var response = await client.Generate.GenerateAsync(new GenerateRequest {
    Model = "llama3",
    Prompt = "Hello, world!"
}, CancellationToken.None);
Console.WriteLine(response.Response);
```

### Speech

```csharp
await using var audio = await client.Audio.CreateSpeechAsync(new SpeechRequest {
    Model = "lfm3-audio",
    Input = "Hello from Ollama.",
    Voice = "female-british-english"
}, cancellationToken);
await using var file = File.Create("hello.wav");
await audio.CopyToAsync(file, cancellationToken);
```

`client.Models.ShowAsync` lists a speech model's voices in `Voices`. Transcribe audio back to text
with `client.Audio.TranscribeAsync`, or translate it with `client.Audio.TranslateAsync`.

### Decision models

```csharp
var decision = await client.Decisions.DecideAsync(new DecisionRequest {
    Model = "d3:2b",
    State = "Our checkout has returned 500 errors since 9am.",
    Questions = new() {
        ["label"] = new ChoiceQuestion {
            Instructions = "Which label fits this ticket?",
            Criteria = new() { ["billing"] = "Payments and refunds", ["bug"] = "Software errors" }
        },
        ["urgent"] = new NoulQuestion { Instructions = "Does this need attention within the hour?" }
    }
}, cancellationToken);
var label = ((ChoiceAnswer)decision.Answers["label"]).Choice;
var urgent = ((NoulAnswer)decision.Answers["urgent"]).Noul; // probability of yes
```

## License
MIT License. See [LICENSE](LICENSE) for details.

## Copyright
Copyright © Panoramic Data Limited 2025

## Contributing
Contributions are welcome! Please open issues or submit pull requests on GitHub.

## Repository
[https://github.com/panoramicdata/Ollama.Api](https://github.com/panoramicdata/Ollama.Api)