using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using ConsoleChat.Tests.TestUtilities;

namespace ConsoleChat.Tests;

public class ListToolsCommandStrategyTests
{
    private const string List = "/list";
    private const string Tools = "tools";

    private static McpToolCollection CreateCollection()
    {
        return McpCollectionFactory.CreateToolCollection();
    }

    [Fact]
    public void GetCompletions_Returns_Command_Name()
    {
        var strategy = new ListToolsCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("", "/l", "");

        Assert.NotNull(completions);
        Assert.Contains(List, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Tools_Option()
    {
        var strategy = new ListToolsCommandStrategy(CreateCollection());
        var completions = strategy.GetCompletions("/list ", "t", "");

        Assert.NotNull(completions);
        Assert.Contains(Tools, completions!);
    }

    [Fact]
    public void CanExecute_Returns_True_For_Command()
    {
        var strategy = new ListToolsCommandStrategy(CreateCollection());
        Assert.True(strategy.CanExecute("/list tools"));
    }

    [Fact]
    public void CanExecute_Returns_False_For_Invalid_Command()
    {
        var strategy = new ListToolsCommandStrategy(CreateCollection());
        Assert.False(strategy.CanExecute("/list"));
    }
}

