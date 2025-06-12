using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat;

public sealed class ChatStreamCommand : AsyncCommand<ChatCommand.Settings>
{
    private readonly IChatClient _chatClient;
    private readonly IChatHistoryService _history;
    private readonly ILogger<ChatStreamCommand> _logger;

    public ChatStreamCommand(IChatClient chatClient, IChatHistoryService history, ILogger<ChatStreamCommand> logger)
    {
        _chatClient = chatClient;
        _history = history;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ChatCommand.Settings settings)
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();
        var tools = toolCollection.Tools;

        AnsiConsole.MarkupLine("Type 'exit' to quit.");

        while (true)
        {
            AnsiConsole.Markup("You: ");
            var input = ChatConsole.ReadMultilineInput();

            if (input is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var userMessage = new ChatMessage(ChatRole.User, input);
            ChatConsole.WriteChatMessages(_history, userMessage);

            await ChatConsole.SendAndDisplayStreamingAsync(_chatClient, _history, tools);
        }

        return 0;
    }
}
