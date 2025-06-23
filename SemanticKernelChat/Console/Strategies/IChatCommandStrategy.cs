namespace SemanticKernelChat.Console;

public interface IChatCommandStrategy
{
    IEnumerable<string>? GetCompletions(string prefix, string word, string suffix);
    bool CanExecute(string input);
    Task<bool> ExecuteAsync(string input, IChatHistoryService history, IChatController controller, IChatConsole console);
}
