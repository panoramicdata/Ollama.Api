# Change Log

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
