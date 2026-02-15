using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System.Net.Http.Headers;
using RaindropServer.Collections;
using RaindropServer.Raindrops;
using RaindropServer.Highlights;
using RaindropServer.Filters;
using RaindropServer.Tags;
using RaindropServer.User;
using RaindropServer.Common;

namespace RaindropServer;

/// <summary>
/// Extension methods for registering Raindrop API services.
/// </summary>
public static class RaindropServiceCollectionExtensions
{
    /// <summary>
    /// Registers Raindrop API clients using configuration from the
    /// "Raindrop" section of <see cref="IConfiguration"/>.
    /// </summary>
    public static IServiceCollection AddRaindropApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RaindropOptions>(configuration.GetSection("Raindrop"));

        // Register core authentication services for clients
        services.AddHttpContextAccessor();
        services.AddTransient<AuthHeaderHandler>();

        // Register default token provider if not already registered (allows tests to override)
        services.AddSingleton<ITokenProvider, HttpContextTokenProvider>();

        var settings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            })
        };

        void Configure(IServiceProvider sp, HttpClient client)
        {
            var options = sp.GetRequiredService<IOptions<RaindropOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("Raindrop BaseUrl is required");
            }

            if (options.TimeoutSeconds <= 0)
            {
                throw new InvalidOperationException("Raindrop TimeoutSeconds must be greater than 0");
            }

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }

        void AddApi<T>() where T : class
        {
            services.AddRefitClient<T>(settings)
                .ConfigureHttpClient(Configure)
                .AddHttpMessageHandler<AuthHeaderHandler>();
        }

        AddApi<ICollectionsApi>();
        AddApi<IRaindropsApi>();
        AddApi<IHighlightsApi>();
        AddApi<IFiltersApi>();
        AddApi<ITagsApi>();
        AddApi<IUserApi>();

        return services;
    }
}
