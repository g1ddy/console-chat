using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private readonly McpServerManager _manager;

    public McpToolCollection(McpServerManager manager)
    {
        _manager = manager;
    }

    public IReadOnlyCollection<string> Servers => _manager.State.Servers;

    public IReadOnlyList<McpClientTool> Tools => _manager.State.GetTools();

    internal IReadOnlyList<McpServerState.McpServerInfo> GetServerInfos() => _manager.State.GetServerInfos();

    public bool IsServerEnabled(string name) => _manager.State.IsServerEnabled(name);

    public void SetServerEnabled(string name, bool enabled) => _manager.SetServerEnabled(name, enabled);

    public async ValueTask DisposeAsync()
    {
        await _manager.DisposeAsync();
    }

    public static async Task<McpToolCollection> CreateAsync(
        IConfiguration configuration,
        ILogger<McpServerState>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var manager = await McpServerManager.CreateAsync(configuration, logger ?? NullLogger<McpServerState>.Instance, cancellationToken);
        return new McpToolCollection(manager);
    }
}
