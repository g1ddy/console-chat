using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleChat.Tests.TestUtilities;
using NSubstitute;
using SemanticKernelChat;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using Xunit;

namespace ConsoleChat.Tests;

public class ToggleMcpServerCommandStrategyTests
{
    private const string Toggle = "/toggle";
    private const string Mcp = "mcp";

    private static McpToolCollection CreateCollection(params string[] servers)
        => McpCollectionFactory.CreateToolCollection(servers);

    [Fact]
    public void GetCompletions_Returns_Command_Name_For_Prefix()
    {
        var strategy = new ToggleMcpServerCommandStrategy(CreateCollection());

        // Partial prefix
        var completions = strategy.GetCompletions("", "/to", "");
        Assert.NotNull(completions);
        Assert.Contains(Toggle, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Null_For_Empty_Input()
    {
        var strategy = new ToggleMcpServerCommandStrategy(CreateCollection());
        // Empty input returns null as expected by strategy logic
        var completions = strategy.GetCompletions("", "", "");
        Assert.Null(completions);
    }

    [Fact]
    public void GetCompletions_Returns_Mcp_Option_For_Toggle_Command()
    {
        var strategy = new ToggleMcpServerCommandStrategy(CreateCollection());

        // With space after command
        var completions = strategy.GetCompletions("/toggle ", "m", "");

        Assert.NotNull(completions);
        Assert.Contains(Mcp, completions!);
    }

    [Fact]
    public void GetCompletions_Returns_Null_For_Unknown_Args()
    {
        var strategy = new ToggleMcpServerCommandStrategy(CreateCollection());

        var completions = strategy.GetCompletions("/toggle mcp ", "extra", "");
        Assert.Null(completions);
    }

    [Theory]
    [InlineData("/toggle mcp")]
    [InlineData("/TOGGLE MCP")]
    [InlineData("/toggle Mcp")]
    public void CanExecute_Returns_True_For_Valid_Command(string input)
    {
        var strategy = new ToggleMcpServerCommandStrategy(CreateCollection());
        Assert.True(strategy.CanExecute(input));
    }

    [Theory]
    [InlineData("/toggle")]        // Missing subcommand
    [InlineData("/toggle other")]  // Wrong subcommand
    [InlineData("/other mcp")]     // Wrong command
    [InlineData("toggle mcp")]     // Missing slash
    [InlineData("/toggle mcp extra")] // Too many args
    public void CanExecute_Returns_False_For_Invalid_Command(string input)
    {
        var strategy = new ToggleMcpServerCommandStrategy(CreateCollection());
        Assert.False(strategy.CanExecute(input));
    }

    [Fact]
    public async Task ExecuteAsync_Updates_Server_State_Based_On_Selection()
    {
        // Arrange
        var server1 = "server1";
        var server2 = "server2";
        var tools = CreateCollection(server1, server2);

        // Initial state: all enabled by default in factory
        Assert.True(tools.IsServerEnabled(server1));
        Assert.True(tools.IsServerEnabled(server2));

        var strategy = new ToggleMcpServerCommandStrategy(tools);
        var console = Substitute.For<IChatConsole>();
        var history = Substitute.For<IChatHistoryService>();
        var controller = Substitute.For<IChatController>();

        // Capture the initial state passed to the prompt to verify it matches the tool collection
        IEnumerable<(string Name, bool Selected)>? capturedItems = null;

        // Mock user selection: keep server1 enabled, disable server2
        // Use Arg.Do to capture and materialize the enumerable immediately (via ToList)
        // to avoid lazy evaluation reflecting future state changes.
        console.PromptMultiSelection(
            Arg.Any<string>(),
            Arg.Do<IEnumerable<(string Name, bool Selected)>>(x => capturedItems = x.ToList()))
            .Returns(new List<string> { server1 });

        // Act
        await strategy.ExecuteAsync("/toggle mcp", history, controller, console);

        // Assert
        Assert.True(tools.IsServerEnabled(server1));   // Selected -> Enabled
        Assert.False(tools.IsServerEnabled(server2));  // Not selected -> Disabled

        // Verify prompt was called with correct initial state (all servers enabled by default)
        Assert.NotNull(capturedItems);
        var itemsList = capturedItems.ToList();
        Assert.Equal(2, itemsList.Count);
        Assert.All(itemsList, i => Assert.True(i.Selected));
        Assert.Contains(itemsList, i => i.Name == server1);
        Assert.Contains(itemsList, i => i.Name == server2);
    }

    [Fact]
    public async Task ExecuteAsync_Handles_Empty_Selection()
    {
        // Arrange
        var server1 = "server1";
        var tools = CreateCollection(server1);

        var strategy = new ToggleMcpServerCommandStrategy(tools);
        var console = Substitute.For<IChatConsole>();

        // Mock user selection: select nothing (disable all)
        console.PromptMultiSelection(Arg.Any<string>(), Arg.Any<IEnumerable<(string, bool)>>())
            .Returns(new List<string>());

        // Act
        await strategy.ExecuteAsync("/toggle mcp", Substitute.For<IChatHistoryService>(), Substitute.For<IChatController>(), console);

        // Assert
        Assert.False(tools.IsServerEnabled(server1));
    }
}
