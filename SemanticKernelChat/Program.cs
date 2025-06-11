using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using Spectre.Console;

using SemanticKernelChat;

// Entry point for the SemanticKernelChat console application
var builder = Host.CreateApplicationBuilder(args);

// Configure logging to output to the console
builder.Logging.AddConsole();

// Register Semantic Kernel chat client and other dependencies
builder.Services.AddSemanticKernelChatClient(builder.Configuration);
builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

// Build the host
var host = builder.Build();

// Resolve the chat client and history service from DI
var chatClient = host.Services.GetRequiredService<IChatClient>();
var history = host.Services.GetRequiredService<IChatHistoryService>();

// Create transports and tools for the chat client
await using var toolCollection = await McpToolCollection.CreateAsync();
var tools = toolCollection.Tools;

Console.WriteLine("Type 'exit' to quit.");

// Main chat loop
while (true)
{
    Console.Write("You: ");
    var input = ReadMultilineInput();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    // Add user message to history
    history.AddUserMessage(input);

    // Get AI response using chat client and tools
    var response = await chatClient.GetResponseAsync(history.Messages, new() { Tools = [.. tools] });
    var reply = response.Text;
    Console.WriteLine($"AI: {reply}");

    // Add assistant message to history
    history.AddAssistantMessage(reply);
}

static string ReadMultilineInput()
{
    var console = AnsiConsole.Console;
    var sb = new StringBuilder();

    while (true)
    {
        var key = console.Input.ReadKey(intercept: true);
        if (key == null)
        {
            continue;
        }

        if (key.Value.Key == ConsoleKey.Enter)
        {
            if (key.Value.Modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                console.WriteLine();
                sb.AppendLine();
                continue;
            }

            console.WriteLine();
            break;
        }

        if (key.Value.Key == ConsoleKey.Backspace)
        {
            if (sb.Length > 0)
            {
                var lastChar = sb[sb.Length - 1];
                sb.Length--;
                if (!char.IsControl(lastChar))
                {
                    console.Cursor.MoveLeft(1);
                    console.Write(" ");
                    console.Cursor.MoveLeft(1);
                }
            }
            continue;
        }

        if (!char.IsControl(key.Value.KeyChar))
        {
            console.Write(key.Value.KeyChar.ToString());
            sb.Append(key.Value.KeyChar);
        }
    }

    return sb.ToString();
}
