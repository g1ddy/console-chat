using ModelContextProtocol.Client;

namespace SemanticKernelChat;

public static class McpClientHelper
{
    public static StdioClientTransport[] CreateTransports()
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../McpServer"));
        return
        [
            new(new()
            {
                Command = "dotnet",
                Arguments = ["run", "--project", projectPath, "--no-build"],
                Name = "McpServer"
            }),
            // Add additional MCP transports here
        ];
    }

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
