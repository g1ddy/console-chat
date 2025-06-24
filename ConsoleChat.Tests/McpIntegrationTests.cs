using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Threading;
using SemanticKernelChat;
using SemanticKernelChat.Infrastructure;

namespace ConsoleChat.Tests;

public class McpIntegrationTests
{
    [Fact]
    public async Task Tools_Are_Exposed_From_McpServer()
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();
        await WaitForToolsAsync(toolCollection, 5);
        var tools = toolCollection.Tools;

        Assert.True(tools.Count >= 5);

        var kernel = Kernel.CreateBuilder().Build();

        foreach (var tool in tools)
        {
#pragma warning disable SKEXP0001 // Experimental API - ok for tests
            string pluginName = $"mcp_{tool.Name}";
            _ = kernel.ImportPluginFromFunctions(pluginName, [tool.AsKernelFunction()]);
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

    [Fact]
    public async Task Tools_Are_Exposed_From_McpServer_From_Arbitrary_Cwd()
    {
        var original = Environment.CurrentDirectory;
        var tempDir = Directory.CreateTempSubdirectory();
        Environment.CurrentDirectory = tempDir.FullName;

        try
        {
            await using var toolCollection = await McpToolCollection.CreateAsync();
            await WaitForToolsAsync(toolCollection, 5);
            Assert.True(toolCollection.Tools.Count >= 5);
        }
        finally
        {
            Environment.CurrentDirectory = original;
            tempDir.Delete(recursive: true);
        }
    }

    private static async Task WaitForToolsAsync(McpToolCollection collection, int count, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs && !cancellationToken.IsCancellationRequested)
        {
            if (collection.Tools.Count >= count)
            {
                break;
            }
            await Task.Delay(50, cancellationToken);
        }
    }
}
