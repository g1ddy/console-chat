using Microsoft.Extensions.AI;
using SemanticKernelChat.Infrastructure;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

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

    public TextCompletionCommand(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Query))
        {
            System.Console.Error.WriteLine("--query is required");
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
                System.Console.WriteLine(message.Text);
        }

        return 0;
    }
}
