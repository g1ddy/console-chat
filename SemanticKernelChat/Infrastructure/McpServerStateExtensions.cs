using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace SemanticKernelChat.Infrastructure;

public static class McpServerStateExtensions
{
    public static IServiceCollection AddMcpServerState(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<McpServerManager>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<McpServerState>>();
            return new McpServerManager(configuration, logger);
        });
        services.AddSingleton(sp => sp.GetRequiredService<McpServerManager>().State);
        services.AddHostedService<McpManagerInitializationService>();
        return services;
    }
}
