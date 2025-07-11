using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly ILogger<McpServerState> _logger;

    private McpServerManager(McpServerState state, Dictionary<string, McpServerConfig> configs, ILogger<McpServerState> logger)
    {
        _state = state;
        _configs = configs;
        _logger = logger;
    }

    internal McpServerManager(McpServerState state)
        : this(state, new Dictionary<string, McpServerConfig>(StringComparer.OrdinalIgnoreCase), NullLogger<McpServerState>.Instance)
    {
    }

    public McpServerManager(IConfiguration configuration, ILogger<McpServerState> logger)
    {
        _logger = logger;
        _configs = new Dictionary<string, McpServerConfig>(
            McpClientHelper.GetServerConfigs(configuration),
            StringComparer.OrdinalIgnoreCase);
        var serversDict = new Dictionary<string, McpServerState.ServerEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, config) in _configs)
        {
            serversDict[name] = new McpServerState.ServerEntry { Enabled = !config.Disabled };
        }
        _state = new McpServerState(serversDict);
    }

    public McpServerState State => _state;

    public static async Task<McpServerManager> CreateAsync(
        IConfiguration configuration,
        ILogger<McpServerState> logger,
        CancellationToken cancellationToken = default)
    {
        var manager = new McpServerManager(configuration, logger);
        await manager.InitializeAsync(cancellationToken);
        return manager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (name, config) in _configs)
        {
            if (!config.Disabled)
            {
                _loadTasks[name] = LoadServerAsync(name, cancellationToken);
            }
        }

        await Task.WhenAll(_loadTasks.Values);
    }

    public void SetServerEnabled(string name, bool enabled)
    {
        if (_state.GetEntry(name) is null)
        {
            _logger.LogWarning("MCP server {ServerName} not found", name);
            return;
        }

        _state.SetServerEnabled(name, enabled);
        if (enabled)
        {
            _loadTasks.GetOrAdd(name, _ => LoadServerAsync(name));
        }
    }

    private async Task LoadServerAsync(string name, CancellationToken cancellationToken = default)
    {
        var entry = _state.GetEntry(name)!;
        var config = _configs[name];
        entry.Status = ServerStatus.Loading;
        try
        {
            var transport = await McpClientHelper.CreateTransportAsync(name, config, cancellationToken: cancellationToken);
            var client = await McpClientFactory.CreateAsync(transport);
            _disposables.Add(client);

            var capabilities = client.ServerCapabilities;

            IList<McpClientTool> tools = Array.Empty<McpClientTool>();
            if (capabilities.Tools is not null)
            {
                tools = await client.ListToolsAsync();
            }

            IList<McpClientPrompt> prompts = Array.Empty<McpClientPrompt>();
            if (capabilities.Prompts is not null)
            {
                prompts = await client.ListPromptsAsync();
            }
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
        catch (Exception ex)
        {
            entry.Status = ServerStatus.Failed;
            entry.FailureReason = ex.Message;
            _logger.LogError(ex, "Error loading MCP server {ServerName}", name);
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
