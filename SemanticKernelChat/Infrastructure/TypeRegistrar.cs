using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Infrastructure;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public TypeRegistrar(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public ITypeResolver Build()
        => new TypeResolver(_services.BuildServiceProvider());

    public void Register(Type? service, Type? implementation)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (implementation is null)
            throw new ArgumentNullException(nameof(implementation));

        _services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type? service, object? implementation)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (implementation is null)
            throw new ArgumentNullException(nameof(implementation));

        _services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type? service, Func<object?> factory)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _services.AddSingleton(service, _ => factory());
    }

    private sealed class TypeResolver : ITypeResolver, IDisposable
    {
        private readonly ServiceProvider _provider;

        public TypeResolver(ServiceProvider provider)
        {
            _provider = provider;
        }

        public object? Resolve(Type? type)
            => _provider.GetService(type);

        public void Dispose()
            => _provider.Dispose();
    }
}
