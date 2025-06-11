using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat;

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

            await ChatConsole.SendAndDisplayAsync(_chatClient, _history, input, tools);
        }

        return 0;
    }


}

