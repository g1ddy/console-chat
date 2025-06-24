// Entry point for the MCP server application
// Sets up dependency injection, logging, and server transports

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to output all logs to stderr at the Trace level
builder.Logging.AddConsole(options =>
{
    // All logs go to stderr for better separation from stdout
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register the MCP server and configure transports and tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

// Build and run the host asynchronously
await builder.Build().RunAsync();
