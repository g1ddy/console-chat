using ModelContextProtocol.Client;

namespace SemanticKernelChat.Infrastructure;

internal enum ServerStatus
{
    None,
    Loading,
    Ready,
    Failed
}

/// <summary>
/// Holds in-memory information about MCP servers.
/// </summary>
public sealed class McpServerState
{
    internal sealed class ServerEntry
    {
        public IList<McpClientTool> Tools { get; } = new List<McpClientTool>();
        public IList<McpClientPrompt> Prompts { get; } = new List<McpClientPrompt>();
        public bool Enabled { get; set; }
        public ServerStatus Status { get; set; } = ServerStatus.None;
    }

    internal sealed record McpServerInfo(string Name, bool Enabled, ServerStatus Status, IReadOnlyList<McpClientTool> Tools);
    internal sealed record McpPromptInfo(string Name, bool Enabled, ServerStatus Status, IReadOnlyList<McpClientPrompt> Prompts);

    private readonly Dictionary<string, ServerEntry> _servers;

    internal McpServerState()
        : this(new Dictionary<string, ServerEntry>(StringComparer.OrdinalIgnoreCase))
    {
    }

    internal McpServerState(Dictionary<string, ServerEntry> servers)
    {
        _servers = servers;
    }

    internal ServerEntry? GetEntry(string name) => _servers.TryGetValue(name, out var entry) ? entry : null;

    public IReadOnlyCollection<string> Servers => _servers.Keys;

    public bool IsServerEnabled(string name) => _servers.TryGetValue(name, out var entry) && entry.Enabled;

    public void SetServerEnabled(string name, bool enabled)
    {
        if (_servers.TryGetValue(name, out var entry))
        {
            entry.Enabled = enabled;
        }
    }

    public IReadOnlyList<McpClientTool> GetTools()
    {
        return _servers.Where(p => p.Value.Enabled && p.Value.Status == ServerStatus.Ready)
            .SelectMany(p => p.Value.Tools)
            .ToList();
    }

    public IReadOnlyList<McpClientPrompt> GetPrompts()
    {
        return _servers.Where(p => p.Value.Enabled && p.Value.Status == ServerStatus.Ready)
            .SelectMany(p => p.Value.Prompts)
            .ToList();
    }

    internal IReadOnlyList<McpServerInfo> GetServerInfos()
    {
        return _servers.Select(p =>
            new McpServerInfo(
                p.Key,
                p.Value.Enabled,
                p.Value.Status,
                p.Value.Enabled && p.Value.Status == ServerStatus.Ready
                    ? p.Value.Tools.ToList()
                    : Array.Empty<McpClientTool>()))
            .ToList();
    }

    internal IReadOnlyList<McpPromptInfo> GetPromptInfos()
    {
        return _servers.Select(p =>
            new McpPromptInfo(
                p.Key,
                p.Value.Enabled,
                p.Value.Status,
                p.Value.Enabled && p.Value.Status == ServerStatus.Ready
                    ? p.Value.Prompts.ToList()
                    : Array.Empty<McpClientPrompt>()))
            .ToList();
    }
}
