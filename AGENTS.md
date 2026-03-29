# Repository Guidelines

This repository contains a console chat application and an example Model Context Protocol (MCP) server. Both are part of the `ConsoleChat.sln` solution targeting .NET 8.

## Building
- Restore dependencies and build from the repository root:
  ```bash
  dotnet restore ConsoleChat.sln
  dotnet build ConsoleChat.sln
  ```

## Testing
- Run all unit and integration tests with:
  ```bash
  dotnet test ConsoleChat.sln -v minimal
  ```
- After running the tests, verify the console UI:
  ```bash
  dotnet run --project SemanticKernelChat text-completion-test
  ```
- Always run the test suite after making any changes.

## Performance Optimization
- Avoid negligible or inconclusive performance optimizations that increase code complexity.
- Prioritize maintainability and idiomatic C# (like LINQ) over manual array manipulations unless there is a significant, measurable bottleneck.
- Use simpler expressions like collection expressions (`[.. result]`) or LINQ where appropriate.

## Running
- Start the MCP server on its own:
  ```bash
  dotnet run --project McpServer
  ```
- Launch the console chat client (which will auto-start the MCP server by default):
  ```bash
  dotnet run --project SemanticKernelChat
  ```
  Run the `text-completion-test` command to verify the console UI:
  ```bash
  dotnet run --project SemanticKernelChat text-completion-test
  ```

These commands should work cross-platform and mirror the steps used in the GitHub workflow (`.github/workflows/dotnet-build.yml`).
