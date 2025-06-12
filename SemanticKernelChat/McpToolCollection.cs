using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;

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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var transports = McpClientHelper.CreateTransports(configuration).ToArray();
        var tasks = transports.Select(async transport =>
        {
            var client = await McpClientFactory.CreateAsync(transport);
            var tools = await client.ListToolsAsync();
            return (client, tools);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (client, tools) in results)
        {
            collection._disposables.Add(client);
            collection._tools.AddRange(tools);
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
