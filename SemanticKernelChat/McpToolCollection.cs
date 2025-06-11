using ModelContextProtocol.Client;
using System.Collections.Generic;
using System.Linq;

namespace SemanticKernelChat;

/// <summary>
/// Holds MCP tools and disposes underlying transports and clients when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private readonly List<McpClientTool> _tools = new();
    private readonly List<IAsyncDisposable> _disposables = new();

    public IReadOnlyList<McpClientTool> Tools => _tools;

    private McpToolCollection() { }

    /// <summary>
    /// Launches MCP servers, retrieves tools, and returns a disposable collection.
    /// </summary>
    public static async Task<McpToolCollection> CreateAsync()
    {
        var collection = new McpToolCollection();

        foreach (var transport in McpClientHelper.CreateTransports())
        {
            var client = await McpClientFactory.CreateAsync(transport);
            collection._disposables.Add(client);
            collection._tools.AddRange(await client.ListToolsAsync());
        }

        return collection;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in Enumerable.Reverse(_disposables))
        {
            await disposable.DisposeAsync();
        }
    }
}
