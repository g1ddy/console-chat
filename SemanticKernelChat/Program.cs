using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System.Collections.Generic;
using SemanticKernelChat;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddSemanticKernelChatClient(builder.Configuration);

builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
var history = host.Services.GetRequiredService<IChatHistoryService>();

StdioClientTransport[] transports =
[
    new(new()
    {
        Command = "dotnet",
        Arguments = ["run", "--project", "../McpServer", "--no-build"],
        Name = "McpServer"
    }),
    // Add additional MCP transports here
];

var tools = await GetToolsAsync(transports);

Console.WriteLine("Type 'exit' to quit.");

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

    history.AddUserMessage(input);

    var response = await chatClient.GetResponseAsync(history.Messages, new() { Tools = [.. tools] });
    var reply = response.Text;
    Console.WriteLine($"AI: {reply}");
    history.AddAssistantMessage(reply);
}

static async Task<IList<McpClientTool>> GetToolsAsync(IEnumerable<StdioClientTransport> transports)
{
    var allTools = new List<McpClientTool>();

    foreach (var transport in transports)
    {
        await using var client = await McpClientFactory.CreateAsync(transport);
        allTools.AddRange(await client.ListToolsAsync());
    }

    return allTools;
}
