using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace SemanticKernelChat.Infrastructure;

public static class McpCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="McpServerState"/>, <see cref="McpToolCollection"/>
    /// and <see cref="McpPromptCollection"/> services using a shared state
    /// instance.
    /// </summary>
    public static IServiceCollection AddMcpCollections(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMcpServerState(configuration);
        services.AddSingleton<McpToolCollection>();
        services.AddSingleton<McpPromptCollection>();
        return services;
    }
}
