using Microsoft.Extensions.Configuration;

using ModelContextProtocol.Client;

using SemanticKernelChat.Helpers;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports and clients when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private readonly Dictionary<string, IList<McpClientTool>> _plugins = new();
    private readonly Dictionary<string, McpServerConfig> _configs = new();
    private readonly Dictionary<string, Task<IList<McpClientTool>>> _loadTasks = new();
    private readonly HashSet<string> _enabledServers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IAsyncDisposable> _disposables = new();

    public IReadOnlyDictionary<string, IList<McpClientTool>> Plugins => _plugins;
    public IReadOnlyCollection<string> Servers => _plugins.Keys;

    public IReadOnlyList<McpClientTool> Tools
    {
        get
        {
            foreach (var name in _enabledServers)
            {
                if (!_loadTasks.ContainsKey(name))
                {
                    _loadTasks[name] = LoadServerAsync(name);
                }
            }

            foreach (var (name, task) in _loadTasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    _plugins[name] = task.Result;
                }
            }

            return _plugins.Where(p => _enabledServers.Contains(p.Key))
                .SelectMany(p => p.Value)
                .ToList();
        }
    }

    public bool IsServerEnabled(string name) => _enabledServers.Contains(name);

    public void SetServerEnabled(string name, bool enabled)
    {
        if (enabled)
        {
            _enabledServers.Add(name);
            if (!_loadTasks.ContainsKey(name))
            {
                _loadTasks[name] = LoadServerAsync(name);
            }
        }
        else
        {
            _enabledServers.Remove(name);
        }
    }

    /// <summary>
    /// Launches MCP servers, retrieves tools, and returns a disposable collection.
    /// </summary>
    public static Task<McpToolCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var collection = new McpToolCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var servers = McpClientHelper.GetServerConfigs(configuration);

        foreach (var (name, config) in servers)
        {
            collection._configs[name] = config;
            collection._plugins[name] = new List<McpClientTool>();
            if (!config.Disabled)
            {
                collection._enabledServers.Add(name);
                collection._loadTasks[name] = collection.LoadServerAsync(name);
            }
        }

        return Task.FromResult(collection);
    }

    private async Task<IList<McpClientTool>> LoadServerAsync(string name)
    {
        var config = _configs[name];
        var transport = await McpClientHelper.CreateTransportAsync(name, config);
        var client = await McpClientFactory.CreateAsync(transport);
        _disposables.Add(client);
        var tools = await client.ListToolsAsync();
        _plugins[name] = tools;
        return tools;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in Enumerable.Reverse(_disposables))
        {
            await disposable.DisposeAsync();
        }
    }
}
