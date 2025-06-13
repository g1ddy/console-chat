using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SemanticKernelChat.Console;
using SemanticKernelChat.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

public sealed class ChatCommand : AsyncCommand<ChatCommand.Settings>
{
    private readonly IChatClient _chatClient;
    private readonly IChatHistoryService _history;
    private readonly ILogger<ChatCommand> _logger;

    public sealed class Settings : CommandSettings
    {
    }

    public ChatCommand(IChatClient chatClient, IChatHistoryService history, ILogger<ChatCommand> logger)
    {
        _chatClient = chatClient;
        _history = history;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await using var toolCollection = await McpToolCollection.CreateAsync();
        var tools = toolCollection.Tools;
        ChatConsole.Initialize(tools);

        AnsiConsole.MarkupLine(CliConstants.WelcomeMessage);

        while (true)
        {
            var (headerText, justify, style) = ChatConsole.GetUserStyle(ChatRole.User);
            var rule = new Rule(headerText) { Justification = justify, Style = style };
            AnsiConsole.Write(rule);

            AnsiConsole.Markup("You: ");
            var input = await ChatConsole.ReadMultilineInputAsync();

            if (input is null ||
                input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            _history.AddUserMessage(input);

            await ChatConsole.SendAndDisplayAsync(_chatClient, _history, tools);
        }

        return 0;
    }
}
