using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds prompts from MCP servers and manages underlying resources.
/// </summary>
public sealed class McpPromptCollection
{
    private readonly McpServerState _state;

    public McpPromptCollection(McpServerState state)
    {
        _state = state;
    }

    public IReadOnlyCollection<string> Servers => _state.Servers;

    public IReadOnlyList<McpClientPrompt> Prompts => _state.GetPrompts();

    internal IReadOnlyList<McpServerState.McpPromptInfo> GetServerInfos() => _state.GetPromptInfos();

    public bool IsServerEnabled(string name) => _state.IsServerEnabled(name);

    public void SetServerEnabled(string name, bool enabled) => _state.SetServerEnabled(name, enabled);

    public static async Task<McpPromptCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var state = await McpServerState.CreateAsync(configuration, cancellationToken);
        return new McpPromptCollection(state);
    }
}
