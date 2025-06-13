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
        ChatConsole.Initialize(tools);

        AnsiConsole.MarkupLine(CliConstants.WelcomeMessage);

        while (true)
        {
            var (style, headerText) = ChatConsole.GetPanelConfig(ChatRole.User);
            var rule = new Rule(headerText) { Style = style };
            AnsiConsole.Write(rule);
            AnsiConsole.Markup("You: ");
            var input = await ChatConsole.ReadMultilineInputAsync();

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

            _history.AddUserMessage(input);

            await ChatConsole.SendAndDisplayStreamingAsync(_chatClient, _history, tools);
        }

        return 0;
    }

}
