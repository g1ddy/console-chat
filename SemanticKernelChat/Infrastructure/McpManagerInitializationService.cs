using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelChat.Infrastructure;

internal sealed class McpManagerInitializationService : IHostedService
{
    private readonly McpServerManager _manager;

    public McpManagerInitializationService(McpServerManager manager)
    {
        _manager = manager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _manager.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
