using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<McpServerState>>();
        var manager = await McpServerManager.CreateAsync(configuration, logger, cancellationToken);
        services.AddSingleton(manager.State);
        services.AddSingleton(manager);
        return services;
    }
}
