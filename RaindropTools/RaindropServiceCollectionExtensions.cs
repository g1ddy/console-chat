using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace RaindropTools;

/// <summary>
/// Extension methods for registering Raindrop API services.
/// </summary>
public static class RaindropServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="RaindropApiClient"/> using configuration from the
    /// "Raindrop" section of <see cref="IConfiguration"/>.
    /// </summary>
    public static IServiceCollection AddRaindropApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RaindropOptions>(configuration.GetSection("Raindrop"));

        services.AddHttpClient<RaindropApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RaindropOptions>>().Value;
            client.BaseAddress = new Uri("https://api.raindrop.io/rest/v1/");
            if (!string.IsNullOrWhiteSpace(options.ApiToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
            }
        });

        return services;
    }
}
