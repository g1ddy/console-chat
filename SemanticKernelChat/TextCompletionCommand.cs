using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System;

namespace SemanticKernelChat;

public sealed class TextCompletionCommand : AsyncCommand<TextCompletionCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--query <QUERY>")]
        public string? Query { get; set; }
    }

    private const string SystemPrompt = """
You're an AI CLI assistant. Treat the rest as a completion request and return only that text with no filler.
When a tool must be called, respond with pure JSON: {"tool_name":"...","parameters":{...}} or an array of such objects.
Never guess tool results or add extra text.
""";

    private readonly IChatClient _chatClient;
    private readonly ILogger<TextCompletionCommand> _logger;

    public TextCompletionCommand(IChatClient chatClient, ILogger<TextCompletionCommand> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Query))
        {
            Console.Error.WriteLine("--query is required");
            return -1;
        }

        await using var toolCollection = await McpToolCollection.CreateAsync();
        var tools = toolCollection.Tools;

        List<ChatMessage> messages =
        [ new(ChatRole.System, SystemPrompt), new(ChatRole.User, settings.Query) ];

        var response = await _chatClient.GetResponseAsync(
            messages,
            new() { Tools = [.. tools] });

        foreach (var message in response.Messages)
        {
            if (message.Role == ChatRole.Assistant)
            {
                Console.WriteLine(message.Text);
            }
        }

        return 0;
    }
}
