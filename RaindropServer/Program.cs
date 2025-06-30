using RaindropTools;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddRaindropApi(builder.Configuration)
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(RaindropTools.RaindropsTools).Assembly);

await builder.Build().RunAsync();
