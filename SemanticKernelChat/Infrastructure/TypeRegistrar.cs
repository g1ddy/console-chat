using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace SemanticKernelChat.Infrastructure;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;
    private readonly HostApplicationBuilder? _builder;
    private IHost? _host;
    private IServiceProvider? _provider;

    public TypeRegistrar(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TypeRegistrar(HostApplicationBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _services = builder.Services;
    }

    /// <summary>
    /// Sets the <see cref="IServiceProvider"/> to use when building the
    /// <see cref="ITypeResolver"/>. If not specified, a new provider is
    /// created from the registered services.
    /// </summary>
    /// <param name="provider">Service provider created elsewhere.</param>
    public void SetServiceProvider(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public ITypeResolver Build()
    {
        if (_provider is not null)
        {
            return new TypeResolver(_provider, false);
        }

        if (_builder is not null)
        {
            _host = _builder.Build();
            // We must block here because Spectre.Console.Cli's ITypeRegistrar.Build() is synchronous,
            // and we need to ensure the IHost and its background services are started before any
            // command can be resolved and executed.
            _host.StartAsync().GetAwaiter().GetResult();
            _provider = _host.Services;
            return new TypeResolver(_provider, true, _host);
        }

        _provider = _services.BuildServiceProvider();
        return new TypeResolver(_provider, true);
    }

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

    private sealed class TypeResolver : ITypeResolver, IAsyncDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly bool _dispose;
        private readonly IHost? _host;

        public TypeResolver(IServiceProvider provider, bool dispose, IHost? host = null)
        {
            _provider = provider;
            _dispose = dispose;
            _host = host;
        }

        public object? Resolve(Type? type)
            => _provider.GetService(type ?? throw new ArgumentNullException());

        public ValueTask DisposeAsync()
        {
            if (_dispose)
            {
                if (_host is not null)
                {
                    return new ValueTask(_host.StopAsync());
                }

                if (_provider is IAsyncDisposable asyncDisposable)
                {
                    return asyncDisposable.DisposeAsync();
                }

                if (_provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            return ValueTask.CompletedTask;
        }
    }
}
