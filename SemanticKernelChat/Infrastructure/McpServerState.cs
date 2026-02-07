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
        public IList<McpClientTool> Tools { get; set; } = Array.Empty<McpClientTool>();
        public IList<McpClientPrompt> Prompts { get; set; } = Array.Empty<McpClientPrompt>();
        public bool Enabled { get; set; }
        public ServerStatus Status { get; set; } = ServerStatus.None;
        public string? FailureReason { get; set; }
    }

    internal sealed record McpServerInfo(string Name, bool Enabled, ServerStatus Status, IList<McpClientTool> Tools);
    internal sealed record McpPromptInfo(string Name, bool Enabled, ServerStatus Status, IList<McpClientPrompt> Prompts);

    // Concurrent dictionary allows safe updates from multiple threads.
    // Entries themselves are not locked, so individual properties may be mutated concurrently.
    // This class is intended to be accessed by background load tasks and UI threads simultaneously.
    private readonly ConcurrentDictionary<string, ServerEntry> _servers;

    private long _version;
    private volatile Tuple<long, IReadOnlyList<McpClientTool>>? _toolsCache;
    private volatile Tuple<long, IReadOnlyList<McpClientPrompt>>? _promptsCache;
    private readonly object _lock = new();

    internal McpServerState()
        : this(new ConcurrentDictionary<string, ServerEntry>(StringComparer.OrdinalIgnoreCase))
    {
    }

    internal McpServerState(IDictionary<string, ServerEntry> servers)
    {
        _servers = servers as ConcurrentDictionary<string, ServerEntry>
            ?? new ConcurrentDictionary<string, ServerEntry>(servers, StringComparer.OrdinalIgnoreCase);
    }

    internal ServerEntry? GetEntry(string name) => _servers.TryGetValue(name, out var entry) ? entry : null;

    public IReadOnlyCollection<string> Servers => _servers.Keys.ToArray();

    public bool IsServerEnabled(string name) => _servers.TryGetValue(name, out var entry) && entry.Enabled;

    public void SetServerEnabled(string name, bool enabled)
    {
        if (_servers.TryGetValue(name, out var entry))
        {
            entry.Enabled = enabled;
            Interlocked.Increment(ref _version);
        }
    }

    internal void UpdateServerStatus(string name, ServerStatus status, string? failureReason = null)
    {
        if (_servers.TryGetValue(name, out var entry))
        {
            entry.Status = status;
            entry.FailureReason = failureReason;
            Interlocked.Increment(ref _version);
        }
    }

    internal void UpdateServerToolsAndPrompts(string name, IList<McpClientTool> tools, IList<McpClientPrompt> prompts)
    {
        if (_servers.TryGetValue(name, out var entry))
        {
            entry.Tools = tools;
            entry.Prompts = prompts;
            Interlocked.Increment(ref _version);
        }
    }

    public IReadOnlyList<McpClientTool> GetTools()
    {
        return GetCachedItems(
            () => _toolsCache,
            value => _toolsCache = value,
            e => e.Tools);
    }

    public IReadOnlyList<McpClientPrompt> GetPrompts()
    {
        return GetCachedItems(
            () => _promptsCache,
            value => _promptsCache = value,
            e => e.Prompts);
    }

    private IReadOnlyList<T> GetCachedItems<T>(
        Func<Tuple<long, IReadOnlyList<T>>?> getCache,
        Action<Tuple<long, IReadOnlyList<T>>> setCache,
        Func<ServerEntry, IEnumerable<T>> selector)
    {
        long currentVersion = Interlocked.Read(ref _version);
        var localCache = getCache();
        if (localCache?.Item1 == currentVersion)
        {
            return localCache.Item2;
        }

        lock (_lock)
        {
            currentVersion = Interlocked.Read(ref _version);
            localCache = getCache();
            if (localCache?.Item1 == currentVersion)
            {
                return localCache.Item2;
            }

            var newItems = _servers.Values
                .Where(e => e.Enabled && e.Status == ServerStatus.Ready)
                .SelectMany(selector)
                .ToList();

            setCache(Tuple.Create(currentVersion, (IReadOnlyList<T>)newItems));
            return newItems;
        }
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
