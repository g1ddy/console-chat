using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Threading.Tasks;

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
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(implementation);

        _ = _services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type? service, object? implementation)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(implementation);

        _ = _services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type? service, Func<object?> factory)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(factory);

        _ = _services.AddSingleton(service, _
            => factory() ?? throw new ArgumentNullException(nameof(factory))
        );
    }

    private sealed class TypeResolver : ITypeResolver, IDisposable, IAsyncDisposable
    {
        private readonly ServiceProvider _provider;

        public TypeResolver(ServiceProvider provider)
        {
            _provider = provider;
        }

        public object? Resolve(Type? type)
            => _provider.GetService(type ?? throw new ArgumentNullException());

        public void Dispose()
            => _provider.DisposeAsync().AsTask().GetAwaiter().GetResult();

        public ValueTask DisposeAsync()
            => _provider.DisposeAsync();
    }
}
