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
        var state = await McpServerState.CreateAsync(cancellationToken);
        services.AddSingleton(state);
        return services;
    }
}
