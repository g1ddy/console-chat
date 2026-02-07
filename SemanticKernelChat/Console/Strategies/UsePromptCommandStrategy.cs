using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Console;

public sealed class UsePromptCommandStrategy : IChatCommandStrategy
{
    private readonly McpPromptCollection _prompts;

    public UsePromptCommandStrategy(McpPromptCollection prompts)
    {
        _prompts = prompts;
    }

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = CommandTokenizer.SplitArguments((prefix + word).TrimStart());
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Use };
        }

        if (tokens.Length == 2 && tokens[0].Equals(CliConstants.Commands.Use, StringComparison.OrdinalIgnoreCase))
        {
            return _prompts.Prompts.Select(p => p.Name).ToList();
        }

        return null;
    }

    public bool CanExecute(string input)
    {
        var tokens = CommandTokenizer.SplitArguments(input);
        return tokens.Length == 2 &&
               tokens[0].Equals(CliConstants.Commands.Use, StringComparison.OrdinalIgnoreCase) &&
               _prompts.Prompts.Any(p => p.Name.Equals(tokens[1], StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var tokens = CommandTokenizer.SplitArguments(input);
        if (tokens.Length < 2)
        {
            return true;
        }

        var prompt = _prompts.Prompts.FirstOrDefault(p => p.Name.Equals(tokens[1], StringComparison.OrdinalIgnoreCase));
        if (prompt is null)
        {
            return true;
        }

        var result = await prompt.GetAsync();

        if (!string.IsNullOrWhiteSpace(result.Description))
        {
            console.WriteLine(result.Description);
        }

        var messages = new List<ChatMessage>();
        foreach (var message in result.Messages)
        {
            string text = message.Content is TextContentBlock textBlock ? textBlock.Text : string.Empty;
            ChatRole role = message.Role switch
            {
                Role.User => ChatRole.User,
                Role.Assistant => ChatRole.Assistant,
                _ => ChatRole.Assistant,
            };
            messages.Add(new ChatMessage(role, text));
        }

        history.Add([.. messages]);

        console.WriteLine($"Sending {prompt.Name} to the agent");

        await controller.SendAndDisplayAsync(history);
        return true;
    }
}
