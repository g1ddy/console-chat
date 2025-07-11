using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using SemanticKernelChat.Helpers;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Handles MCP server lifecycle and populates <see cref="McpServerState"/>.
/// </summary>
public sealed class McpServerManager : IAsyncDisposable
{
    private readonly McpServerState _state;
    private readonly Dictionary<string, McpServerConfig> _configs;
    private readonly ConcurrentDictionary<string, Task> _loadTasks = new();
    private readonly List<IAsyncDisposable> _disposables = new();

    private McpServerManager(McpServerState state, Dictionary<string, McpServerConfig> configs)
    {
        _state = state;
        _configs = configs;
    }

    internal McpServerManager(McpServerState state)
        : this(state, new Dictionary<string, McpServerConfig>(StringComparer.OrdinalIgnoreCase))
    {
    }

    public McpServerState State => _state;

    public static async Task<McpServerManager> CreateAsync(CancellationToken cancellationToken = default)
    {
        var loader = new McpServerConfigLoader();
        var configs = new Dictionary<string, McpServerConfig>(loader.Load(), StringComparer.OrdinalIgnoreCase);
        var serversDict = new Dictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase);
        var state = new McpServerState(serversDict);
        var manager = new McpServerManager(state, configs);

        foreach (var (name, config) in configs)
        {
            serversDict[name] = new McpServerState.ServerEntry { Enabled = !config.Disabled };
            if (!config.Disabled)
            {
                manager._loadTasks[name] = manager.LoadServerAsync(name);
            }
        }

        await Task.WhenAll(manager._loadTasks.Values);
        return manager;
    }

    public void SetServerEnabled(string name, bool enabled)
    {
        _state.SetServerEnabled(name, enabled);
        if (enabled)
        {
            _loadTasks.GetOrAdd(name, _ => LoadServerAsync(name));
        }
    }

    private async Task LoadServerAsync(string name)
    {
        var entry = _state.GetEntry(name)!;
        var config = _configs[name];
        entry.Status = ServerStatus.Loading;
        try
        {
            var transport = await McpClientHelper.CreateTransportAsync(name, config);
            var client = await McpClientFactory.CreateAsync(transport);
            _disposables.Add(client);
            var tools = await client.ListToolsAsync();
            var prompts = await client.ListPromptsAsync();
            entry.Tools.Clear();
            foreach (var tool in tools)
            {
                entry.Tools.Add(tool);
            }
            entry.Prompts.Clear();
            foreach (var prompt in prompts)
            {
                entry.Prompts.Add(prompt);
            }
            entry.Status = ServerStatus.Ready;
        }
        catch
        {
            entry.Status = ServerStatus.Failed;
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in Enumerable.Reverse(_disposables))
        {
            await disposable.DisposeAsync();
        }
    }
}
