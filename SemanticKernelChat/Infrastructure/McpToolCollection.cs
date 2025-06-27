using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports when no longer needed.
/// </summary>
public sealed class McpToolCollection
{
    private readonly McpServerState _state;

    public McpToolCollection(McpServerState state)
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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var state = await McpServerState.CreateAsync(configuration, cancellationToken);
        return new McpToolCollection(state);
    }
}
