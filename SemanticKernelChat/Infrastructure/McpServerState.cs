using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using SemanticKernelChat.Helpers;
using System.Collections.Concurrent;

namespace SemanticKernelChat.Infrastructure;

internal enum ServerStatus
{
    None,
    Loading,
    Ready,
    Failed
}

internal sealed class McpServerState : IAsyncDisposable
{
    internal sealed class ServerEntry
    {
        public IList<McpClientTool> Tools { get; } = new List<McpClientTool>();
        public IList<McpClientPrompt> Prompts { get; } = new List<McpClientPrompt>();
        public bool Enabled { get; set; }
        public ServerStatus Status { get; set; } = ServerStatus.None;
    }

    private readonly Dictionary<string, ServerEntry> _servers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, McpServerConfig> _configs = new();
    private readonly ConcurrentDictionary<string, Task> _loadTasks = new();
    private readonly List<IAsyncDisposable> _disposables = new();

    private McpServerState() { }

    public IReadOnlyCollection<string> Servers => _servers.Keys;

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

    public IReadOnlyList<McpClientTool> GetTools()
    {
        TriggerLoads();
        return _servers.Where(p => p.Value.Enabled && p.Value.Status == ServerStatus.Ready)
            .SelectMany(p => p.Value.Tools)
            .ToList();
    }

    public IReadOnlyList<McpClientPrompt> GetPrompts()
    {
        TriggerLoads();
        return _servers.Where(p => p.Value.Enabled && p.Value.Status == ServerStatus.Ready)
            .SelectMany(p => p.Value.Prompts)
            .ToList();
    }

    private void TriggerLoads()
    {
        foreach (var (name, entry) in _servers)
        {
            if (entry.Enabled && entry.Status == ServerStatus.None)
            {
                _loadTasks.GetOrAdd(name, _ => LoadServerAsync(name));
            }
        }
    }

    private async Task LoadServerAsync(string name)
    {
        var entry = _servers[name];
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

    public static async Task<McpServerState> CreateAsync(CancellationToken cancellationToken = default)
    {
        var state = new McpServerState();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var servers = McpClientHelper.GetServerConfigs(configuration);

        foreach (var (name, config) in servers)
        {
            state._configs[name] = config;
            state._servers[name] = new ServerEntry { Enabled = !config.Disabled };
            if (!config.Disabled)
            {
                state._loadTasks[name] = state.LoadServerAsync(name);
            }
        }

        await Task.WhenAll(state._loadTasks.Values);
        return state;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in Enumerable.Reverse(_disposables))
        {
            await disposable.DisposeAsync();
        }
    }
}
