using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.AI;
using SemanticKernelChat;
using SemanticKernelChat.Commands;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using ConsoleChat.Tests.TestUtilities;
using Spectre.Console.Testing;
using Spectre.Console.Cli;

namespace ConsoleChat.Tests;

public class ChatCommandTests
{
    private sealed class FakeLineEditor : IChatLineEditor
    {
        private readonly Queue<string?> _inputs;

        public FakeLineEditor(IEnumerable<string?> inputs)
        {
            _inputs = new Queue<string?>(inputs);
        }

        public Task<string?> ReadLine(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inputs.Count > 0 ? _inputs.Dequeue() : null);
        }
    }



    [Fact]
    public async Task ExecuteAsync_Writes_Welcome_And_Response()
    {
        var testConsole = new TestConsole();
        var lineEditor = new FakeLineEditor(new[] { "hi", null });
        var chatConsole = new ChatConsole(lineEditor, testConsole);
        var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "done")) };
        var controller = new ChatController(chatConsole, client, McpCollectionFactory.CreateToolCollection(), []);
        var history = new ChatHistoryService();
        var command = new ChatCommand(history, controller, chatConsole, Enumerable.Empty<IChatCommandStrategy>());

        var context = new CommandContext(new FakeRemainingArguments(), "chat", new object());

        _ = await command.ExecuteAsync(context, new ChatCommandBase.Settings());

        Assert.Contains("Welcome to ConsoleChat", testConsole.Output);
        Assert.Contains("done", testConsole.Output);
    }
}

