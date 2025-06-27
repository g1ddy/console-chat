using Microsoft.Extensions.Configuration;
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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var state = await McpServerState.CreateAsync(configuration, cancellationToken);
        services.AddSingleton(state);
        return services;
    }
}
