# Console Chat with MCP

This repository contains a console based chat client built with **Microsoft Semantic Kernel** and an example **Model Context Protocol (MCP)** server. The chat client can talk to language models or fall back to a simple echo bot. The MCP server exposes simple C# functions as tools that can be invoked from the chat.

## Projects

- **SemanticKernelChat** – Console application that hosts the chat client and automatically starts any configured MCP server.
- **McpServer** – Example MCP server exposing a few utility tools.
- **ConsoleChat.Tests** – Unit tests covering the tool implementations and MCP integration.

Both applications target **.NET 8** and are included in the `ConsoleChat.sln` solution file.

### Solution layout

```
ConsoleChat.sln
├── SemanticKernelChat/     # main chat client
│   ├── Commands/           # Spectre.Console CLI commands
│   ├── Console/            # UI and controller classes
│   ├── Clients/            # implementations of IChatClient
│   ├── Infrastructure/     # MCP helpers and DI extensions
│   └── Helpers/            # provider utilities
├── McpServer/              # example MCP server with tools
└── ConsoleChat.Tests/      # xUnit test project
```

This layout keeps the chat client, tool server, and tests separated while still
belonging to the same solution. Exploring each folder is a good way to get
familiar with the architecture.

## Building

Restore dependencies and build all projects:

```bash
# from the repository root
 dotnet restore ConsoleChat.sln
 dotnet build ConsoleChat.sln
```

Run the unit tests with:

```bash
 dotnet test ConsoleChat.sln -v minimal
```

## Running the MCP server

The server can be started on its own and listens via standard input/output so that clients can connect using the MCP protocol:

```bash
 dotnet run --project McpServer
```

It registers all tools in the `McpServer` assembly. Example tools include `CurrentTime`, `ToUpper`, and `Add`.

The chat client itself exposes a few rich rendering helpers implemented as Semantic Kernel functions:

- `RenderTable` – shows a `Table` of fruit counts.
- `RenderTree` – displays a small `Tree` structure.
- `RenderChart` – renders a `BarChart` of fruit sales.

When invoked from chat these functions write directly to the console via `ChatConsole`.

## Running the chat client

Launch the console chat application with:

```bash
 dotnet run --project SemanticKernelChat
```

The Spectre.Console CLI registers commands that can be invoked by name. For
example the default command `chat-stream` can be launched explicitly:

```bash
dotnet run --project SemanticKernelChat chat-stream
```

The client starts any configured MCP transports (by default it launches the `McpServer` project) and then enters an interactive chat loop. Commands available are:

- `chat-stream` *(default)* – Streaming responses as they are generated.
- `chat` – Waits for the full response before printing.
- `text-completion` – Single-shot completion via `--query`.

Type `exit` on an empty line to quit.

The editor remembers your previous inputs. Use the **Up** and **Down** arrows to
cycle through earlier messages. Set the `CHAT_HISTORY_FILE` environment variable
to persist this history across sessions.

### Spectre Console

The console client relies on [Spectre.Console](https://spectreconsole.net/) for
rich terminal output such as panels, spinners, and command parsing. Specify the
desired command alias when launching the application. Running without an alias
starts `chat`.

```bash
dotnet publish SemanticKernelChat -c Release -o out
./out/SemanticKernelChat chat
./out/SemanticKernelChat
```

### Architecture and Clean Code

The console application follows a simple Model-View-Controller pattern.
`ChatConsole` is responsible purely for rendering chat messages and
reading user input (the **View**). All orchestration logic that calls
the AI client lives in `ChatController` (the **Controller**), which
interacts with `IChatHistoryService` as the **Model**. This separation
makes the UI independent of the underlying chat provider and keeps each
class focused on a single responsibility.

### Configuration

`SemanticKernelChat` uses the standard .NET configuration system. Place an `appsettings.json` file next to the executable or in the project directory to configure the underlying chat provider. When no provider is configured an echo bot is used.

Example `appsettings.json`:

```json
{
  "Provider": "OpenAI",
  "OPENAI_API_KEY": "your-api-key",
  "OpenAI": {
    "ModelId": "gpt-3.5-turbo"
  },
  "Ollama": {
    "ModelId": "llama3",
    "BaseUrl": "http://localhost:11434"
  },
  "AwsBedrock": {
    "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0",
    "RoleArn": "arn:aws:iam::123456789012:role/BedrockAccess"
  }
}
```

Set `Provider` to `OpenAI`, `Ollama`, or `AwsBedrock` and adjust the nested section with the required options. For OpenAI the API key may also be supplied via the `OPENAI_API_KEY` environment variable. The `Ollama` section uses the [OllamaSharp](https://www.nuget.org/packages/OllamaSharp) package and accepts an optional `BaseUrl` for remote servers.

The MCP server itself only uses the default logging configuration. Its `appsettings.json` looks like:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Getting started in the codebase

Key entry points include:

- `Program.cs` in **SemanticKernelChat** – wires up the command-line host and dependency injection.
- `ChatController` – orchestrates messages between the console UI and the configured chat client.
- `McpServer` project – exposes example tools that can be invoked from chat.

Exploring these components is a good way to understand how the client and server communicate. The test project contains additional usage examples and is a helpful reference when extending the toolset.

## License

This project is licensed under the [MIT License](LICENSE).
