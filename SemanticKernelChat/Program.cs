using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SemanticKernelChat;
using SemanticKernelChat.Commands;
using SemanticKernelChat.Infrastructure;
using SemanticKernelChat.Console;
using RadLine;

using Spectre.Console;
using Spectre.Console.Cli;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

await builder.Services.AddSemanticKernelChatClient(builder.Configuration);
builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();

await builder.Services.AddMcpCollections();

var console = AnsiConsole.Console;
builder.Services.AddSingleton(console);

builder.Services.AddSingleton<IChatCommandStrategy, ExitCommandStrategy>();
builder.Services.AddSingleton<IChatCommandStrategy, ToggleMcpServerCommandStrategy>();
builder.Services.AddSingleton<IChatCommandStrategy, SetMcpServerStateCommandStrategy>();
builder.Services.AddSingleton<IChatCommandStrategy, ListToolsCommandStrategy>();
builder.Services.AddSingleton<IChatCommandStrategy, ListPromptsCommandStrategy>();
builder.Services.AddSingleton<IChatCommandStrategy, UsePromptCommandStrategy>();

builder.Services.AddSingleton<ITextCompletion, CommandCompletion>();

builder.Services.AddSingleton<IChatLineEditor, ChatLineEditor>();
builder.Services.AddSingleton<IChatConsole, ChatConsole>();
builder.Services.AddSingleton<IChatController, ChatController>();

var registrar = new TypeRegistrar(builder.Services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    _ = config.SetExceptionHandler(ex => console.WriteException(ex, ExceptionFormats.ShortenTypes));
    _ = config.AddCommand<TextCompletionTestCommand>("text-completion-test");
    _ = config.AddCommand<TextCompletionCommand>("text-completion");
    _ = config.AddCommand<ChatStreamCommand>("chat-stream");
    _ = config.AddCommand<ChatCommand>("chat");
});

app.SetDefaultCommand<ChatCommand>();

return await app.RunAsync(args);
