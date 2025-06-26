using ModelContextProtocol.Client;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports when no longer needed.
/// </summary>
public sealed class McpToolCollection : IDisposable, IAsyncDisposable
{
    private readonly McpServerState _state;

    private McpToolCollection()
    {
        _state = (McpServerState)Activator.CreateInstance(typeof(McpServerState), nonPublic: true)!;
    }

    private McpToolCollection(McpServerState state)
    {
        _state = state;
    }

    public IReadOnlyCollection<string> Servers => _state.Servers;

    public IReadOnlyList<McpClientTool> Tools => _state.GetTools();

    internal IReadOnlyList<McpServerState.McpServerInfo> GetServerInfos() => _state.GetServerInfos();

    public bool IsServerEnabled(string name) => _state.IsServerEnabled(name);

    public void SetServerEnabled(string name, bool enabled) => _state.SetServerEnabled(name, enabled);

    public static async Task<McpToolCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var state = await McpServerState.CreateAsync(cancellationToken);
        return new McpToolCollection(state);
    }

    public void Dispose() => _state.DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync() => await _state.DisposeAsync();
}
