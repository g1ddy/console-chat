using System;
using System.IO;
using SemanticKernelChat;
using SemanticKernelChat.Commands;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using Microsoft.Extensions.AI;
using System.Linq;
using ConsoleChat.Tests.TestUtilities;
using Spectre.Console.Testing;
using Spectre.Console.Cli;

namespace ConsoleChat.Tests;

public class FileCommandTests
{

    [Fact]
    public async Task ExecuteAsync_Reads_File_And_Sends_To_Client()
    {
        string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
        await File.WriteAllTextAsync(file, "hello world");
        try
        {
            var history = new ChatHistoryService();
            var console = new TestConsole();
            var lineEditor = new ChatLineEditor(new CommandCompletion(Enumerable.Empty<IChatCommandStrategy>()));
            var chatConsole = new ChatConsole(lineEditor, console);
            var client = new FakeChatClient { Response = new(new ChatMessage(ChatRole.Assistant, "done")) };
            var controller = new ChatController(chatConsole, client, McpCollectionFactory.CreateToolCollection());

            var command = new FileCommand(history, controller);
            var context = new CommandContext(new FakeRemainingArguments(), "file", new object());
            var settings = new ChatCommandBase.Settings { FilePath = file };

            _ = await command.ExecuteAsync(context, settings);

            Assert.NotNull(client.LastMessages);
            var msg = client.LastMessages!.Last();
            Assert.Contains("summarize", msg.Text, StringComparison.OrdinalIgnoreCase);
            var dataContent = msg.Contents.OfType<DataContent>().Single();
            Assert.Contains("name=", dataContent.MediaType, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(file);
        }
    }
}
