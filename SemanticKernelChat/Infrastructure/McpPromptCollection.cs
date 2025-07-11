using ModelContextProtocol.Client;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds prompts from MCP servers and manages underlying resources.
/// </summary>
public sealed class McpPromptCollection
{
    private readonly McpServerManager _manager;

    public McpPromptCollection(McpServerManager manager)
    {
        _manager = manager;
    }

    public IReadOnlyCollection<string> Servers => _manager.State.Servers;

    public IReadOnlyList<McpClientPrompt> Prompts => _manager.State.GetPrompts();

    internal IReadOnlyList<McpServerState.McpPromptInfo> GetServerInfos() => _manager.State.GetPromptInfos();

    public bool IsServerEnabled(string name) => _manager.State.IsServerEnabled(name);

    public void SetServerEnabled(string name, bool enabled) => _manager.SetServerEnabled(name, enabled);

    public static async Task<McpPromptCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var manager = await McpServerManager.CreateAsync(cancellationToken);
        return new McpPromptCollection(manager);
    }
}
