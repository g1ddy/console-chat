using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
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
    public static string ResolveCommandPath(string command)
    {
        if (Path.IsPathFullyQualified(command))
        {
            return command;
        }

        if (command.Contains(Path.DirectorySeparatorChar) ||
            command.Contains(Path.AltDirectorySeparatorChar) ||
            Path.HasExtension(command))
        {
            return Path.GetFullPath(command, AppContext.BaseDirectory);
        }

        return command;
    }

    internal static IDictionary<string, McpServerConfig> GetServerConfigs(IConfiguration configuration)
    {
        return configuration.GetSection("McpServers")
            .Get<Dictionary<string, McpServerConfig>>()
            ?? new Dictionary<string, McpServerConfig>();
    }

    internal static async Task<IClientTransport> CreateTransportAsync(
        string name,
        McpServerConfig serverConfig,
        IHttpClientFactory? httpClientFactory = null,
        CancellationToken cancellationToken = default)
    {
        var transportType = serverConfig.TransportType ?? McpServerTypes.Stdio;

        switch (transportType.ToLowerInvariant())
        {
            case McpServerTypes.Stdio:
                string command = ResolveCommandPath(serverConfig.Command);
                return new StdioClientTransport(new()
                {
                    Command = command,
                    Arguments = serverConfig.Arguments,
                    EnvironmentVariables = serverConfig.EnvironmentVariables,
                    WorkingDirectory = AppContext.BaseDirectory,
                });
            case McpServerTypes.Sse:
                return new HttpClientTransport(new()
                {
                    Endpoint = new Uri(serverConfig.Command)
                });
            default:
                throw new InvalidOperationException($"Unsupported server type: {serverConfig.TransportType}");
        }
    }

    public static async IAsyncEnumerable<IClientTransport> CreateTransportsAsync(
        IConfiguration configuration,
        IHttpClientFactory? httpClientFactory = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var servers = GetServerConfigs(configuration);

        if (servers.Count == 0)
        {
            yield break;
        }

        foreach (var server in servers)
        {
            var serverName = server.Key;
            var serverConfig = server.Value ?? throw new InvalidOperationException($"Server configuration for '{serverName}' is missing.");

            if (serverConfig.Disabled)
            {
                continue;
            }

            yield return await CreateTransportAsync(serverName, serverConfig, httpClientFactory, cancellationToken);
        }
    }
}
