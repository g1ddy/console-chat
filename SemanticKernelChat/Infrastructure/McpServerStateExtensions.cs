using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelChat.Infrastructure;

public static class McpServerStateExtensions
{
    public static async Task<IServiceCollection> AddMcpServerState(
        this IServiceCollection services,
        CancellationToken cancellationToken = default)
    {
        var manager = await McpServerManager.CreateAsync(cancellationToken);
        services.AddSingleton(manager.State);
        services.AddSingleton(manager);
        return services;
    }
}
