using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelChat.Infrastructure;

public static class McpCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="McpServerState"/>, <see cref="McpToolCollection"/>
    /// and <see cref="McpPromptCollection"/> services using a shared state
    /// instance.
    /// </summary>
    public static async Task<IServiceCollection> AddMcpCollections(
        this IServiceCollection services,
        CancellationToken ct = default)
    {
        var state = await McpServerState.CreateAsync(ct);
        services.AddSingleton(state);
        services.AddSingleton<McpToolCollection>(_ => new McpToolCollection(state));
        services.AddSingleton<McpPromptCollection>(_ => new McpPromptCollection(state));
        return services;
    }
}
