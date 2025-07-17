using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;

namespace SemanticKernelChat.Infrastructure;

public static class ChatCommandServiceCollectionExtensions
{
    /// <summary>
    /// Registers all <see cref="Console.IChatCommandStrategy"/> implementations found in the
    /// <c>SemanticKernelChat</c> assembly as singletons.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    public static IServiceCollection AddChatCommandStrategies(this IServiceCollection services)
    {
        var strategyType = typeof(SemanticKernelChat.Console.IChatCommandStrategy);
        var assembly = strategyType.Assembly;

        foreach (var type in assembly.GetTypes()
            .Where(t => strategyType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract))
        {
            services.AddSingleton(strategyType, type);
        }

        return services;
    }
}
