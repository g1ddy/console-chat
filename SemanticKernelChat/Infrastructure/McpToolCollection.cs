using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ModelContextProtocol.Client;

using SemanticKernelChat.Helpers;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports and clients when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private readonly Dictionary<string, IList<McpClientTool>> _plugins = new();
    private readonly HashSet<string> _enabledServers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IAsyncDisposable> _disposables = new();

    public IReadOnlyDictionary<string, IList<McpClientTool>> Plugins => _plugins;
    public IReadOnlyCollection<string> Servers => _plugins.Keys;

    public IReadOnlyList<McpClientTool> Tools =>
        _plugins.Where(p => _enabledServers.Contains(p.Key)).SelectMany(p => p.Value).ToList();

    public bool IsServerEnabled(string name) => _enabledServers.Contains(name);

    public void SetServerEnabled(string name, bool enabled)
    {
        if (enabled)
        {
            _enabledServers.Add(name);
        }
        else
        {
            _enabledServers.Remove(name);
        }
    }

    /// <summary>
    /// Launches MCP servers, retrieves tools, and returns a disposable collection.
    /// </summary>
    public static async Task<McpToolCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var collection = new McpToolCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var services = new ServiceCollection();
        _ = services.AddHttpClient();
        using var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

        var transports = new List<IClientTransport>();
        await foreach (var transport in McpClientHelper.CreateTransportsAsync(configuration, httpClientFactory, cancellationToken))
        {
            transports.Add(transport);
        }

        var tasks = transports.Select(async transport =>
        {
            var client = await McpClientFactory.CreateAsync(transport);
            var tools = await client.ListToolsAsync();
            return (client, transport.Name, tools);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (client, name, tools) in results)
        {
            collection._disposables.Add(client);
            collection._plugins[name] = tools;
            collection._enabledServers.Add(name);
        }

        return collection;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in Enumerable.Reverse(_disposables))
        {
            await disposable.DisposeAsync();
        }
    }
}
