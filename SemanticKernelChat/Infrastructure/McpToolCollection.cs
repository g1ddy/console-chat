using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

using ModelContextProtocol.Client;

using SemanticKernelChat.Helpers;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports and clients when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private sealed class ServerEntry
    {
        public IList<McpClientTool> Tools { get; } = new List<McpClientTool>();
        public bool Enabled { get; set; }
    }

    private readonly Dictionary<string, ServerEntry> _servers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, McpServerConfig> _configs = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Task<IList<McpClientTool>>> _loadTasks = new();
    private readonly List<IAsyncDisposable> _disposables = new();

    public IReadOnlyDictionary<string, IList<McpClientTool>> Plugins => _servers.ToDictionary(p => p.Key, p => p.Value.Tools);
    public IReadOnlyCollection<string> Servers => _servers.Keys;

    public IReadOnlyList<McpClientTool> Tools
    {
        get
        {
            foreach (var (name, entry) in _servers)
            {
                if (entry.Enabled)
                {
                    _loadTasks.GetOrAdd(name, _ => LoadServerAsync(name));
                }
            }

            foreach (var (name, task) in _loadTasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    _servers[name].Tools.Clear();
                    foreach (var tool in task.Result)
                    {
                        _servers[name].Tools.Add(tool);
                    }
                }
            }

            return _servers.Where(p => p.Value.Enabled)
                .SelectMany(p => p.Value.Tools)
                .ToList();
        }
    }

    public bool IsServerEnabled(string name) => _servers.TryGetValue(name, out var entry) && entry.Enabled;

    public void SetServerEnabled(string name, bool enabled)
    {
        if (_servers.TryGetValue(name, out var entry))
        {
            entry.Enabled = enabled;
            if (enabled)
            {
                _loadTasks.GetOrAdd(name, _ => LoadServerAsync(name));
            }
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
            collection._servers[name] = new ServerEntry { Enabled = !config.Disabled };
            if (!config.Disabled)
            {
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
        _servers[name].Tools.Clear();
        foreach (var tool in tools)
        {
            _servers[name].Tools.Add(tool);
        }
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
