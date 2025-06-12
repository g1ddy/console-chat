using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;
using System.Net;

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
                case McpServerTypes.Stdio:
                    yield return new StdioClientTransport(new()
                    {
                        Name = server.Name,
                        Command = server.Command,
                        Arguments = server.Arguments,
                        EnvironmentVariables = server.EnvironmentVariables,
                    });
                    break;
                case McpServerTypes.Sse:
                    using (var http = new HttpClient())
                    {
                        var response = http.Send(new HttpRequestMessage(HttpMethod.Head, server.Command));
                        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                        {
                            throw new InvalidOperationException($"SSE endpoint '{server.Command}' requires authentication.");
                        }
                    }

                    yield return new SseClientTransport(new()
                    {
                        Name = server.Name,
                        Endpoint = new Uri(server.Command),
                        TransportMode = HttpTransportMode.Sse,
                    });
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported server type: {server.Type}");
            }
        }
    }
}
