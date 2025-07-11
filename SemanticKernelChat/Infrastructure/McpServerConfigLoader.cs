using Microsoft.Extensions.Configuration;
using SemanticKernelChat.Helpers;

namespace SemanticKernelChat.Infrastructure;

/// <summary>
/// Loads MCP server configuration from <c>appsettings.json</c>.
/// </summary>
internal sealed class McpServerConfigLoader
{
    public IDictionary<string, McpServerConfig> Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return McpClientHelper.GetServerConfigs(configuration);
    }
}
