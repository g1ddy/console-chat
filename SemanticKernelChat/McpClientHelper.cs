using ModelContextProtocol.Client;
using System.Collections.Generic;

namespace SemanticKernelChat;

public static class McpClientHelper
{
    public static StdioClientTransport[] CreateTransports() =>
    [
        new(new()
        {
            Command = "dotnet",
            Arguments = ["run", "--project", "../McpServer", "--no-build"],
            Name = "McpServer"
        }),
        // Add additional MCP transports here
    ];

    public static async Task<IList<McpClientTool>> GetToolsAsync(IEnumerable<StdioClientTransport> transports)
    {
        var allTools = new List<McpClientTool>();

        foreach (var transport in transports)
        {
            await using var client = await McpClientFactory.CreateAsync(transport);
            allTools.AddRange(await client.ListToolsAsync());
        }

        return allTools;
    }
}
