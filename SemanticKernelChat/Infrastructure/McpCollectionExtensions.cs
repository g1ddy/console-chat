using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        await services.AddMcpServerState(configuration, ct);
        services.AddSingleton<McpToolCollection>();
        services.AddSingleton<McpPromptCollection>();
        return services;
    }
}
