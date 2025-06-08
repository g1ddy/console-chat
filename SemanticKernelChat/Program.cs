using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using SemanticKernelChat;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddChatClient(_ =>
    new OpenAI.Chat.ChatClient(
        builder.Configuration["OPENAI_MODEL"] ?? "gpt-3.5-turbo",
        builder.Configuration["OPENAI_API_KEY"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ).AsIChatClient());

builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
var history = host.Services.GetRequiredService<IChatHistoryService>();

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

    var response = await chatClient.GetResponseAsync(history.Messages);
    var reply = response.Text;
    Console.WriteLine($"AI: {reply}");
    history.AddAssistantMessage(reply);
}
