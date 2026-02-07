using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using SemanticKernelChat.Infrastructure;

namespace SemanticKernelChat.Console;

public sealed class SuggestPromptsCommandStrategy : IChatCommandStrategy
{
    private readonly IChatClient _chatClient;
    private readonly McpPromptCollection _prompts;

    public SuggestPromptsCommandStrategy(IChatClient chatClient, McpPromptCollection prompts)
    {
        _chatClient = chatClient;
        _prompts = prompts;
    }

    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix)
    {
        var tokens = CommandTokenizer.SplitArguments((prefix + word).TrimStart());
        if (tokens.Length == 1)
        {
            return new[] { CliConstants.Commands.Suggest };
        }
        return null;
    }

    public bool CanExecute(string input)
    {
        return string.Equals(input.Trim(), CliConstants.Commands.Suggest, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console)
    {
        var requestMessages = history.Messages.ToList();
        string promptText = BuildSuggestionPrompt();
        requestMessages.Add(new ChatMessage(ChatRole.User, promptText));

        ChatResponse response = await _chatClient.GetResponseAsync(requestMessages);
        string? text = response.Messages.LastOrDefault()?.Text;
        IEnumerable<string> suggestions = ParseSuggestions(text);

        foreach (string name in suggestions)
        {
            var prompt = _prompts.Prompts.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (prompt is null)
            {
                continue;
            }

            try
            {
                var result = await prompt.GetAsync();
                if (!string.IsNullOrWhiteSpace(result.Description))
                {
                    console.WriteLine(result.Description);
                }
                foreach (var message in result.Messages)
                {
                    string messageText = message.Content is TextContentBlock textBlock ? textBlock.Text : string.Empty;
                    console.WriteLine(messageText);
                }
            }
            catch (Exception ex)
            {
                // Prompts might fail to be retrieved. Inform the user and log for diagnostics.
                console.DisplayError(ex);
            }
        }

        return true;
    }

    private string BuildSuggestionPrompt()
    {
        var names = string.Join(", ", _prompts.Prompts.Select(p => p.Name));
        return $"Suggest three prompts from the following list that best continue the conversation. Only list the names separated by newlines: {names}";
    }

    private static IEnumerable<string> ParseSuggestions(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }
        var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return lines.Select(l => Regex.Replace(l.Trim(), @"^\s*(\d+\.|[-*])\s*", "")).Where(l => l.Length > 0).Take(3);
    }
}
