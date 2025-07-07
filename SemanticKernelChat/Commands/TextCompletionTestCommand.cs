using Microsoft.Extensions.AI;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using SemanticKernelChat.Plugins;
using Microsoft.SemanticKernel;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernelChat.Commands;

/// <summary>
/// Demonstration command that runs through a scripted chat interaction. It uses
/// the standard chat loop from <see cref="ChatCommandBase"/> but feeds the user
/// input from a mocked line editor so the conversation runs automatically.
/// </summary>
public sealed class TextCompletionTestCommand : ChatCommandBase
{
    /// <summary>
    /// Simple line editor returning predefined input lines.
    /// </summary>
    private sealed class FakeLineEditor : IChatLineEditor
    {
        private readonly Queue<string?> _inputs;

        public FakeLineEditor(IEnumerable<string?> inputs)
        {
            _inputs = new Queue<string?>(inputs);
        }

        public Task<string?> ReadLine(CancellationToken cancellationToken)
        {
            _inputs.TryDequeue(out var input);
            return Task.FromResult(input);
        }
    }

    private static readonly string[] ScriptedInputs =
    [
        $"{CliConstants.Commands.Debug} on",
        "This is a demo of non-streaming chat!",
        "This is a demo of streaming chat!",
        $"{CliConstants.Commands.Use} BugReport",
        CliConstants.Commands.Exit
    ];

    private int _sendCount;

    public TextCompletionTestCommand(
        IChatHistoryService history,
        IChatClient chatClient,
        McpToolCollection tools,
        IEnumerable<IChatCommandStrategy> strategies,
        IAnsiConsole ansiConsole)
        : this(history, CreateController(chatClient, tools, ansiConsole), strategies)
    {
    }

    private TextCompletionTestCommand(
        IChatHistoryService history,
        (IChatController controller, IChatConsole console) created,
        IEnumerable<IChatCommandStrategy> strategies)
        : base(history, created.controller, created.console, strategies)
    {
    }

    private static (IChatController controller, IChatConsole console) CreateController(
        IChatClient chatClient,
        McpToolCollection tools,
        IAnsiConsole ansiConsole)
    {
        var chatConsole = new ChatConsole(new FakeLineEditor(ScriptedInputs), ansiConsole);
        var functions = new RenderableFunctions(chatConsole);

        var tableData = new[]
        {
            new RenderableFunctions.ItemCount("Apples", 12),
            new RenderableFunctions.ItemCount("Bananas", 7)
        };

        var leaf = new RenderableFunctions.TreeNode("Leaf");
        var branch1 = new RenderableFunctions.TreeNode("Branch 1");
        branch1.AddChild(leaf);
        var branch2 = new RenderableFunctions.TreeNode("Branch 2");
        var treeData = new RenderableFunctions.TreeNode("Root");
        treeData.AddChild(branch1);
        treeData.AddChild(branch2);

        var chartData = new[]
        {
            new RenderableFunctions.ChartItem("Apples", 12, Color.Red),
            new RenderableFunctions.ChartItem("Bananas", 7, Color.Yellow)
        };

        var kernelFunctions = new[]
        {
            KernelFunctionFactory.CreateFromMethod(() => functions.SampleTable(tableData)),
            KernelFunctionFactory.CreateFromMethod(() => functions.SampleTree(treeData)),
            KernelFunctionFactory.CreateFromMethod(() => functions.SampleChart(chartData, "Fruit Sales"))
        };

        var plugin = KernelPluginFactory.CreateFromFunctions("RenderableFunctions", kernelFunctions);
#pragma warning disable SKEXP0001
        var kernel = Kernel.CreateBuilder().Build();
        var aiFunctions = plugin.AsAIFunctions(kernel).ToList();
#pragma warning restore SKEXP0001
        var controller = new ChatController(chatConsole, chatClient, tools, aiFunctions);
        return (controller, chatConsole);
    }

    /// <summary>
    /// Sends the next chat message in the scripted sequence using both
    /// non-streaming and streaming display methods.
    /// </summary>
    protected override async Task SendAndDisplayAsync()
    {
        switch (_sendCount++)
        {
            case 0:
                // The first message demonstrates non-streaming chat.
                await Controller.SendAndDisplayAsync(History);
                break;
            default:
                // Subsequent messages demonstrate streaming chat.
                await Controller.SendAndDisplayStreamingAsync(History);
                break;
        }
    }
}
