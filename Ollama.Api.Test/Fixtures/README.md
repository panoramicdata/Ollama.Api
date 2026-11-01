# Fixtures

Responses captured from a local Ollama 0.40.13 server on 2026-11-01, used by the tests in `Offline/`
so they run without a server.

- String values longer than 300 characters (licence texts, chat templates) are cut to 80 characters
  followed by `[trimmed]`. Keys, types and structure are as the server sent them.
- `chat-think-logprobs.json` keeps the first three of its 200 `logprobs` entries.
- `speech-start.wav` is the first 4 KB of the WAV `lfm3-audio` returned for "Hello from Panoramic Data.".
- The `speech-*.json` files are the `400` bodies for an unknown voice, an unsupported format and a
  model that cannot speak.
