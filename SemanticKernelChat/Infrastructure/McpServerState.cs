using System.Collections.Concurrent;
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
///
/// The state is accessed by background loading tasks as well as the UI thread.
/// A <see cref="ConcurrentDictionary{TKey,TValue}"/> is used so reads and writes
/// to the server collection are thread-safe.
/// </summary>
public sealed class McpServerState
{
    internal sealed class ServerEntry
    {
        public IList<McpClientTool> Tools { get; } = new List<McpClientTool>();
        public IList<McpClientPrompt> Prompts { get; } = new List<McpClientPrompt>();
        public bool Enabled { get; set; }
        public ServerStatus Status { get; set; } = ServerStatus.None;
        public string? FailureReason { get; set; }
    }

    internal sealed record McpServerInfo(string Name, bool Enabled, ServerStatus Status, IReadOnlyList<McpClientTool> Tools);
    internal sealed record McpPromptInfo(string Name, bool Enabled, ServerStatus Status, IReadOnlyList<McpClientPrompt> Prompts);

    // Concurrent dictionary allows safe updates from multiple threads.
    // Entries themselves are not locked, so individual properties may be mutated concurrently.
    // This class is intended to be accessed by background load tasks and UI threads simultaneously.
    private readonly ConcurrentDictionary<string, ServerEntry> _servers;

    internal McpServerState()
        : this(new ConcurrentDictionary<string, ServerEntry>(StringComparer.OrdinalIgnoreCase))
    {
    }

    internal McpServerState(ConcurrentDictionary<string, ServerEntry> servers)
    {
        _servers = servers;
    }

    internal ServerEntry? GetEntry(string name) => _servers.TryGetValue(name, out var entry) ? entry : null;

    public IReadOnlyCollection<string> Servers => _servers.Keys.ToArray();

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
