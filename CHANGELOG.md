# Change Log

* 2026-11-01 : 1.1.x
	* Supports Ollama 0.40.13.
	* Added Audio: text to speech (`/v1/audio/speech`), streamed as it is generated, plus
	  transcription and translation, with streamed transcription.
	* Added Decisions: decision models through `/v1/systemone`, with choice, yes/no and score questions.
	* Added Embeddings.EmbedAsync for `/api/embed`, with dimensions.
	* Added Models.BlobExistsAsync and Models.PushBlobAsync, and the current create fields (files,
	  renderer, parser, quantize, capabilities and more).
	* Added Utility.GetUsageAsync and Utility.GetBalanceAsync for Ollama cloud.
	* ShowModelResponse now carries capabilities, thinking values, voices, renderer, parser,
	  template source, model and projector info; tags and loaded models carry digest, details,
	  memory use and context length.
	* Think accepts a level such as "high" as well as true or false; chat and generate accept
	  logprobs and top_logprobs, and Format accepts a JSON schema object. Responses report
	  prompt_eval_cached_count, and generate reports thinking and done_reason.
	* Fixed Models.DeleteAsync, which sent POST and always failed with 405; it now sends DELETE.
	* Requests about a model send `model` rather than the deprecated `name`. `Name` remains as an
	  obsolete alias.
	* The Debug request and response log no longer reads audio, multipart or event-stream bodies.
* 2026-08-31 : 1.0.x
	* Added OllamaClientOptions.ApiKey, sent as a bearer token. Optional: nothing is sent when it is
	  absent, so a local Ollama needing no authentication is unaffected.
	* Added OllamaClientOptions.Timeout, defaulting to the previous hardcoded 30 minutes. A zero or
	  negative value is rejected rather than applied.
* 2025-07-04 : 0.9.24
	* Removed ToolCalls from root of ChatResponse.
* 2025-07-04 : 0.9.24
	* Fixed MCP support for Agentic models/clients.
* 2025-07-04 : 0.9.23
	* Fixed MCP support for Agentic models/clients.
* 2025-06-24 : 0.9.19
	* Added MCP support for Agentic models/clients.
* 2025-06-23 : 0.9.18
	* Fixed bad default GenerateRequest.Format value
* 2025-06-23 : 0.9.14
	* Fixed logo
* 2025-06-23 : 0.9.12
	* Added unit tests for multi-modal requests including images.
	* e.g. using Llava for image description.
* 2025-06-22 : 0.9.8
	* Initial release.
