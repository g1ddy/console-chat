using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System.Text.Json;
using SemanticKernelChat;

namespace ConsoleChat.Tests;

public class McpIntegrationTests
{
    [Fact]
    public async Task Tools_Are_Exposed_From_McpServer()
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();
        var tools = toolCollection.Tools;

        Assert.True(tools.Count >= 5);

        var kernel = Kernel.CreateBuilder().Build();

        foreach (var tool in tools)
        {
#pragma warning disable SKEXP0001 // Experimental API - ok for tests
            string pluginName = $"mcp_{tool.Name}";
            kernel.ImportPluginFromFunctions(pluginName, [tool.AsKernelFunction()]);
#pragma warning restore SKEXP0001

            if (tool.Name == "CurrentTime")
            {
                var result = await kernel.InvokeAsync(pluginName, "CurrentTime");
                var element = result.GetValue<JsonElement>();
                var text = element.GetProperty("content")[0].GetProperty("text").GetString();
                Assert.True(DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out _));
            }
        }
    }
}
