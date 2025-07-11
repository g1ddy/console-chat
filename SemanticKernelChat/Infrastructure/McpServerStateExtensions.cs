using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelChat.Infrastructure;

public static class McpServerStateExtensions
{
    public static async Task<IServiceCollection> AddMcpServerState(
        this IServiceCollection services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var manager = await McpServerManager.CreateAsync(configuration, cancellationToken);
        services.AddSingleton(manager.State);
        services.AddSingleton(manager);
        return services;
    }
}
