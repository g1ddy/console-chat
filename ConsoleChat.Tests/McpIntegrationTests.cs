using SemanticKernelChat;

namespace ConsoleChat.Tests;

public class McpIntegrationTests
{
    [Fact]
    public async Task Tools_Are_Exposed_From_McpServer()
    {
        var transports = McpClientHelper.CreateTransports();
        var tools = await McpClientHelper.GetToolsAsync(transports);

        Assert.True(tools.Count >= 5);
    }
}
