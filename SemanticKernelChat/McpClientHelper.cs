using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Linq;

namespace SemanticKernelChat;

internal sealed record McpServerConfig
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Command { get; init; }
    public string[]? Arguments { get; init; }
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
                    var args = server.Arguments?.ToList() ?? new List<string>();
                    var projectArgIndex = args.FindIndex(a => a == "--project");
                    if (projectArgIndex >= 0 && projectArgIndex + 1 < args.Count)
                    {
                        args[projectArgIndex + 1] = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, args[projectArgIndex + 1]));
                    }

                    yield return new StdioClientTransport(new()
                    {
                        Command = server.Command,
                        Arguments = args,
                        Name = server.Name
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
                        Endpoint = new Uri(server.Command),
                        TransportMode = HttpTransportMode.Sse,
                        Name = server.Name
                    });
                    break;
            }
        }
    }

    public static async Task<IList<McpClientTool>> GetToolsAsync(IEnumerable<IClientTransport> transports)
    {
        var tasks = transports.Select(async transport =>
        {
            await using var client = await McpClientFactory.CreateAsync(transport);
            return await client.ListToolsAsync();
        });

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList();
    }
}
