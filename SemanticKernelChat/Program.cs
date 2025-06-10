using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
var transports = McpClientHelper.CreateTransports();
var tools = await McpClientHelper.GetToolsAsync(transports);

Console.WriteLine("Type 'exit' to quit.");

// Main chat loop
while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
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
