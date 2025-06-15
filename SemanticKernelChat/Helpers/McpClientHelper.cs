using System.Net;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Helpers;

internal sealed record McpServerConfig
{
    public required string Command { get; init; }
    public string? TransportType { get; init; }
    public bool Disabled { get; init; } = false;

    [ConfigurationKeyName("args")]
    public string[]? Arguments { get; init; }

    [ConfigurationKeyName("Env")]
    public Dictionary<string, string?>? EnvironmentVariables { get; init; }
}

public static class McpClientHelper
{
    public static IEnumerable<IClientTransport> CreateTransports(IConfiguration configuration)
    {
        var servers = configuration.GetSection("McpServers").Get<Dictionary<string, McpServerConfig>>() ?? [];
        foreach (var server in servers)
        {
            var serverName = server.Key;
            var serverConfig = server.Value ?? throw new InvalidOperationException($"Server configuration for '{serverName}' is missing.");
            var transportType = serverConfig.TransportType ?? McpServerTypes.Stdio; // Default to Stdio if not specified

            if (serverConfig.Disabled)
            {
                System.Console.WriteLine($"Skipping disabled MCP server: {serverName}");
                continue;
            }

            switch (transportType.ToLowerInvariant())
            {
                case McpServerTypes.Stdio:
                    yield return new StdioClientTransport(new()
                    {
                        Name = serverName,
                        Command = serverConfig.Command,
                        Arguments = serverConfig.Arguments,
                        EnvironmentVariables = serverConfig.EnvironmentVariables,
                    });
                    break;
                case McpServerTypes.Sse:
                    using (var http = new HttpClient())
                    {
                        var response = http.Send(new HttpRequestMessage(HttpMethod.Head, serverConfig.Command));
                        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                        {
                            throw new InvalidOperationException($"SSE endpoint '{serverConfig.Command}' requires authentication.");
                        }
                    }

                    yield return new SseClientTransport(new()
                    {
                        Name = serverName,
                        Endpoint = new Uri(serverConfig.Command),
                        TransportMode = HttpTransportMode.Sse,
                    });
                    break;
            }
        }
    }
}
