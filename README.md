# Console Chat with MCP

This repository contains a console based chat client built with **Microsoft Semantic Kernel** and an example **Model Context Protocol (MCP)** server. The chat client can talk to language models or fall back to a simple echo bot. The MCP server exposes simple C# functions as tools that can be invoked from the chat.

## Projects

- **SemanticKernelChat** – Console application that hosts the chat client and automatically starts any configured MCP server.
- **McpServer** – Example MCP server exposing a few utility tools.
- **ConsoleChat.Tests** – Unit tests covering the tool implementations and MCP integration.

Both applications target **.NET 8** and are included in the `ConsoleChat.sln` solution file.

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

### Spectre Console

The console client relies on [Spectre.Console](https://spectreconsole.net/) for
rich terminal output such as panels, spinners, and command parsing. Three commands
are registered with the Spectre CLI:

- `chat-stream` *(default)*
- `chat`
- `text-completion`

When the application is published you can execute the binary followed by the
desired command alias. Running without an alias starts `chat-stream`.

```bash
dotnet publish SemanticKernelChat -c Release -o out
./out/SemanticKernelChat chat
./out/SemanticKernelChat
```

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
  "AwsBedrock": {
    "ModelId": "anthropic.claude-3-sonnet-20240229-v1:0",
    "RoleArn": "arn:aws:iam::123456789012:role/BedrockAccess"
  }
}
```

Set `Provider` to `OpenAI` or `AwsBedrock` and adjust the nested section with the required options. For OpenAI the API key may also be supplied via the `OPENAI_API_KEY` environment variable.

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

## License

This project is licensed under the [MIT License](LICENSE).
