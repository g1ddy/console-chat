using ModelContextProtocol.Client;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private readonly McpServerState _state;
    private readonly bool _ownsState;

    internal McpToolCollection(McpServerState state)
        : this(state, ownsState: false)
    {
    }

    private McpToolCollection(McpServerState state, bool ownsState)
    {
        _state = state;
        _ownsState = ownsState;
    }

    public IReadOnlyCollection<string> Servers => _state.Servers;

    public IReadOnlyList<McpClientTool> Tools => _state.GetTools();

    internal IReadOnlyList<McpServerState.McpServerInfo> GetServerInfos() => _state.GetServerInfos();

    public bool IsServerEnabled(string name) => _state.IsServerEnabled(name);

    public void SetServerEnabled(string name, bool enabled) => _state.SetServerEnabled(name, enabled);

    public static async Task<McpToolCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var state = await McpServerState.CreateAsync(cancellationToken);
        return new McpToolCollection(state, ownsState: true);
    }

    public async ValueTask DisposeAsync()
    {
        if (_ownsState)
        {
            await _state.DisposeAsync();
        }
    }
}
