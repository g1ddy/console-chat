using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace SemanticKernelChat;

public static class SemanticKernelExtensions
{
    public static IServiceCollection AddSemanticKernelChatClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddChatClient(_ =>
            new OpenAI.Chat.ChatClient(
                configuration["OPENAI_MODEL"] ?? "gpt-3.5-turbo",
                configuration["OPENAI_API_KEY"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            )
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build());

        return services;
    }
}
