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
builder.Services.AddSingleton<IChatLineEditor, ChatLineEditor>();
builder.Services.AddSingleton<IChatConsole, ChatConsole>();
builder.Services.AddSingleton<IChatController, ChatController>();

var registrar = new TypeRegistrar(builder.Services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    _ = config.SetExceptionHandler(ex => AnsiConsole.WriteException(ex, ExceptionFormats.ShortenTypes));
    _ = config.AddCommand<TextCompletionCommand>("text-completion");
    _ = config.AddCommand<ChatStreamCommand>("chat-stream");
    _ = config.AddCommand<ChatCommand>("chat");
});

app.SetDefaultCommand<ChatCommand>();

return await app.RunAsync(args);
