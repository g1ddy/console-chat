using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

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

    [ConfigurationKeyName("env")]
    public Dictionary<string, string?>? EnvironmentVariables { get; init; }
}

public static class McpClientHelper
{
    public static async IAsyncEnumerable<IClientTransport> CreateTransportsAsync(
        IConfiguration configuration,
        IHttpClientFactory? httpClientFactory = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var servers = configuration.GetSection("McpServers")
            .Get<Dictionary<string, McpServerConfig>>();

        if (servers is null || servers.Count == 0)
        {
            System.Console.WriteLine("No MCP servers configured. Please check your configuration.");
            yield break;
        }

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
                        WorkingDirectory = AppContext.BaseDirectory,
                    });
                    break;
                case McpServerTypes.Sse:
                    var http = httpClientFactory?.CreateClient() ?? new HttpClient();
                    try
                    {
                        var response = await http.SendAsync(
                            new HttpRequestMessage(HttpMethod.Head, serverConfig.Command),
                            cancellationToken);
                        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                        {
                            throw new InvalidOperationException($"SSE endpoint '{serverConfig.Command}' requires authentication.");
                        }
                    }
                    finally
                    {
                        if (httpClientFactory is null)
                        {
                            http.Dispose();
                        }
                    }

                    yield return new SseClientTransport(new()
                    {
                        Name = serverName,
                        Endpoint = new Uri(serverConfig.Command),
                        TransportMode = HttpTransportMode.Sse,
                    });
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported server type: {serverConfig.TransportType}");
            }
        }
    }
}
