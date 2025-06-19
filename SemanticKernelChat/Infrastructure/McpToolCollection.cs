using ModelContextProtocol.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemanticKernelChat.Helpers;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Holds MCP tools and disposes underlying transports and clients when no longer needed.
/// </summary>
public sealed class McpToolCollection : IAsyncDisposable
{
    private readonly List<McpClientTool> _tools = new();
    private readonly Dictionary<string, IList<McpClientTool>> _plugins = new();
    private readonly List<IAsyncDisposable> _disposables = new();

    public IReadOnlyList<McpClientTool> Tools => _tools;
    public IReadOnlyDictionary<string, IList<McpClientTool>> Plugins => _plugins;

    private McpToolCollection() { }

    /// <summary>
    /// Launches MCP servers, retrieves tools, and returns a disposable collection.
    /// </summary>
    public static async Task<McpToolCollection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var collection = new McpToolCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var services = new ServiceCollection();
        _ = services.AddHttpClient();
        using var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

        var transports = new List<IClientTransport>();
        await foreach (var transport in McpClientHelper.CreateTransportsAsync(configuration, httpClientFactory, cancellationToken))
        {
            transports.Add(transport);
        }
        var tasks = transports.Select(async transport =>
        {
            var client = await McpClientFactory.CreateAsync(transport);
            var tools = await client.ListToolsAsync();
            return (client, transport.Name, tools);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (client, name, tools) in results)
        {
            collection._disposables.Add(client);
            collection._tools.AddRange(tools);
            collection._plugins[name] = tools;
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
