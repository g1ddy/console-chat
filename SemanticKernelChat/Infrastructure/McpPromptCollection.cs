using ModelContextProtocol.Client;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds prompts from MCP servers and manages underlying resources.
/// </summary>
public sealed class McpPromptCollection : IAsyncDisposable
{
    private readonly McpServerState _state;
    private readonly bool _ownsState;

    internal McpPromptCollection(McpServerState state)
        : this(state, ownsState: false)
    {
    }

    private McpPromptCollection(McpServerState state, bool ownsState)
    {
        _state = state;
        _ownsState = ownsState;
    }

    public IReadOnlyCollection<string> Servers => _state.Servers;

    public IReadOnlyList<McpClientPrompt> Prompts => _state.GetPrompts();

    internal IReadOnlyList<McpServerState.McpPromptInfo> GetServerInfos() => _state.GetPromptInfos();

    public bool IsServerEnabled(string name) => _state.IsServerEnabled(name);

    public void SetServerEnabled(string name, bool enabled) => _state.SetServerEnabled(name, enabled);

    public static async Task<McpPromptCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var state = await McpServerState.CreateAsync(cancellationToken);
        return new McpPromptCollection(state, ownsState: true);
    }

    public async ValueTask DisposeAsync()
    {
        if (_ownsState)
        {
            await _state.DisposeAsync();
        }
    }
}
