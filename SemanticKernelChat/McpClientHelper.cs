using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;

namespace SemanticKernelChat;

internal sealed record McpServerConfig
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Command { get; init; }
    public string[]? Arguments { get; init; }
    public Dictionary<string, string?>? EnvironmentVariables { get; init; }
}

public static class McpClientHelper
{
    public static IEnumerable<IClientTransport> CreateTransports(IConfiguration configuration)
    {
        var servers = configuration.GetSection("McpServers").Get<McpServerConfig[]>() ?? [];
        foreach (var server in servers)
        {
            switch (server.Type.ToLowerInvariant())
            {
                case "stdio":
                    yield return new StdioClientTransport(new()
                    {
                        Name = server.Name,
                        Command = server.Command,
                        Arguments = server.Arguments,
                        EnvironmentVariables = server.EnvironmentVariables,
                    });
                    break;
                case "sse":
                    yield return new SseClientTransport(new()
                    {
                        Name = server.Name,
                        Endpoint = new Uri(server.Command),
                        TransportMode = HttpTransportMode.Sse,
                    });
                    break;
            }
        }
    }

    public static async Task<IList<McpClientTool>> GetToolsAsync(IEnumerable<IClientTransport> transports)
    {
        var allTools = new List<McpClientTool>();

        foreach (var transport in transports)
        {
            await using var client = await McpClientFactory.CreateAsync(transport);
            allTools.AddRange(await client.ListToolsAsync());
        }

        return allTools;
    }
}
