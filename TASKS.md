# Project Tasks

This file tracks planned enhancements for the console chat application and the example MCP server.

## Feature ideas

Below is a list of open features still planned for future development. Earlier tasks have been implemented in another branch and were removed from this file.

1. **Plugin architecture for tools**
   - Allow loading additional MCP tool assemblies at runtime without recompiling `McpServer`.
   - Investigate reflection or `AssemblyLoadContext` to discover tools.

2. **Voice input/output mode**
   - Integrate speech recognition for user input and text-to-speech for responses.
   - Likely requires optional dependencies and should be toggled via configuration.

3. **Session persistence**
   - Save chat history and server state to disk and reload on startup so conversations resume across runs.
   - `ChatHistoryService` and `McpServerState` are key classes to extend.

