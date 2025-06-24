using System.Collections.Generic;
using ModelContextProtocol.Client;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using System.Reflection;
using Xunit;

namespace ConsoleChat.Tests;

public class SetMcpServerStateCommandStrategyTests
{
    private const string Enable = "/enable";
    private const string Disable = "/disable";
    private const string Mcp = "mcp";
    private static McpToolCollection CreateCollection(params string[] servers)
    {
        var collection = new McpToolCollection();

        var serversField = typeof(McpToolCollection).GetField("_servers", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = (System.Collections.IDictionary)serversField.GetValue(collection)!;
        var entryType = serversField.FieldType.GenericTypeArguments[1];
        foreach (var name in servers)
        {
            var entry = Activator.CreateInstance(entryType)!;
            entryType.GetProperty("Enabled")!.SetValue(entry, true);
            dict[name] = entry;
        }

        return collection;
    }

    [Fact]
    public void GetCompletions_Returns_Command_Names()
    {
        var tools = CreateCollection("first");
        var strategy = new SetMcpServerStateCommandStrategy(tools);

        var completions = strategy.GetCompletions("", "/e", "");

        Assert.NotNull(completions);
        Assert.Contains(Enable, completions!);
        Assert.Contains(Disable, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Mcp_Option()
    {
        var tools = CreateCollection("first");
        var strategy = new SetMcpServerStateCommandStrategy(tools);

        var completions = strategy.GetCompletions("/enable ", "m", "");

        Assert.NotNull(completions);
        Assert.Contains(Mcp, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Server_Names()
    {
        var tools = CreateCollection("first", "second");
        var strategy = new SetMcpServerStateCommandStrategy(tools);

        var completions = strategy.GetCompletions("/enable mcp ", "f", "");

        Assert.NotNull(completions);
        Assert.Contains("first", completions!);
        Assert.Contains("second", completions!);
    }

    [Fact]
    public void CanExecute_Returns_True_For_Valid_Command()
    {
        var tools = CreateCollection("first");
        var strategy = new SetMcpServerStateCommandStrategy(tools);

        Assert.True(strategy.CanExecute("/enable mcp first"));
    }

    [Fact]
    public void CanExecute_Returns_False_For_Invalid_Server()
    {
        var tools = CreateCollection("first");
        var strategy = new SetMcpServerStateCommandStrategy(tools);

        Assert.False(strategy.CanExecute("/enable mcp missing"));
    }

    [Fact]
    public void CanExecute_Returns_False_For_Incomplete_Command()
    {
        var tools = CreateCollection("first");
        var strategy = new SetMcpServerStateCommandStrategy(tools);

        Assert.False(strategy.CanExecute("/enable mcp"));
    }
}
