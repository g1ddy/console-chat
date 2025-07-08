using System.Collections.Generic;
using SemanticKernelChat.Console;
using SemanticKernelChat.Plugins;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace ConsoleChat.Tests;

public class RenderableFunctionsTests
{
    private static ChatConsole CreateConsole(TestConsole testConsole)
    {
        var completion = new CommandCompletion(System.Linq.Enumerable.Empty<IChatCommandStrategy>());
        return new ChatConsole(new ChatLineEditor(completion), testConsole);
    }

    [Fact]
    public void RenderTable_Writes_Panel_With_Title()
    {
        var testConsole = new TestConsole();
        var console = CreateConsole(testConsole);
        var functions = new RenderableFunctions(console);
        _ = functions.RenderTable([new RenderableFunctions.ItemCount("Apple", 1)]);
        Assert.Contains("Items", testConsole.Output);
        Assert.Contains("Apple", testConsole.Output);
    }

    [Fact]
    public void RenderTree_Writes_Panel_With_Title()
    {
        var testConsole = new TestConsole();
        var console = CreateConsole(testConsole);
        var functions = new RenderableFunctions(console);
        var root = new RenderableFunctions.TreeNode("Root");
        root.AddChild(new RenderableFunctions.TreeNode("Child"));
        _ = functions.RenderTree(root);
        Assert.Contains("Tree", testConsole.Output);
        Assert.Contains("Root", testConsole.Output);
        Assert.Contains("Child", testConsole.Output);
    }

    [Fact]
    public void RenderChart_Writes_Panel_With_Title()
    {
        var testConsole = new TestConsole();
        var console = CreateConsole(testConsole);
        var functions = new RenderableFunctions(console);
        const string title = "MyChart";
        _ = functions.RenderChart([
            new RenderableFunctions.ChartItem("A", 5, Color.Red)
        ], title);
        Assert.Contains(title, testConsole.Output);
        Assert.Contains("A", testConsole.Output);
    }
}
