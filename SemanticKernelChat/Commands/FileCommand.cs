using Microsoft.Extensions.AI;

using SemanticKernelChat.Console;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Commands;

/// <summary>
/// Command that sends a PDF file to the chat client for summarization.
/// </summary>
public sealed class FileCommand : AsyncCommand<ChatCommandBase.Settings>
{
    private readonly IChatHistoryService _history;
    private readonly IChatController _controller;

    public FileCommand(IChatHistoryService history, IChatController controller)
    {
        _history = history;
        _controller = controller;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ChatCommandBase.Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.FilePath))
        {
            AnsiConsole.MarkupLine("[red]--file is required[/]");
            return -1;
        }

        if (!File.Exists(settings.FilePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {settings.FilePath}[/]");
            return -1;
        }

        if (Path.GetExtension(settings.FilePath) is not ".pdf")
        {
            AnsiConsole.MarkupLine("[red]Only PDF files are supported[/]");
            return -1;
        }

        byte[] data;
        try
        {
            data = await File.ReadAllBytesAsync(settings.FilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AnsiConsole.MarkupLine($"[red]Error reading file: {ex.Message}[/]");
            return -1;
        }

        const string prompt = "Please read the attached file, summarize it, and be ready to answer questions.";
        var mediaType = $"application/pdf;name={Path.GetFileName(settings.FilePath)}";
        var contents = new AIContent[]
        {
            new TextContent(prompt),
            new DataContent(data, mediaType)
        };

        _history.Add(new ChatMessage(ChatRole.User, contents));

        await _controller.SendAndDisplayAsync(_history);
        return 0;
    }
}
