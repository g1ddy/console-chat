using System.Collections.Generic;
using SemanticKernelChat.Console;
using SemanticKernelChat.Plugins;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace ConsoleChat.Tests;

public class RenderableFunctionsTests
{
    private readonly TestConsole _testConsole;
    private readonly RenderableFunctions _functions;

    public RenderableFunctionsTests()
    {
        _testConsole = new TestConsole();
        var completion = new CommandCompletion(System.Linq.Enumerable.Empty<IChatCommandStrategy>());
        var console = new ChatConsole(new ChatLineEditor(completion), _testConsole);
        _functions = new RenderableFunctions(console);
    }

    [Fact]
    public void RenderTable_Writes_Panel_With_Title()
    {
        _ = _functions.RenderTable([new RenderableFunctions.ItemCount("Apple", 1)]);
        Assert.Contains("Items", _testConsole.Output);
        Assert.Contains("Apple", _testConsole.Output);
    }

    [Fact]
    public void RenderTree_Writes_Panel_With_Title()
    {
        var root = new RenderableFunctions.TreeNode("Root");
        root.AddChild(new RenderableFunctions.TreeNode("Child"));
        _ = _functions.RenderTree(root);
        Assert.Contains("Tree", _testConsole.Output);
        Assert.Contains("Root", _testConsole.Output);
        Assert.Contains("Child", _testConsole.Output);
    }

    [Fact]
    public void RenderChart_Writes_Panel_With_Title()
    {
        const string title = "MyChart";
        _ = _functions.RenderChart([
            new RenderableFunctions.ChartItem("A", 5, Color.Red)
        ], title);
        Assert.Contains("Chart", _testConsole.Output);
        Assert.Contains("A", _testConsole.Output);
    }
}
