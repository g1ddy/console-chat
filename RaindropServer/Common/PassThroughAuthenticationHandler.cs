using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RaindropServer.User;

namespace RaindropServer.Common;

public class PassThroughAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IMemoryCache _cache;

    public PassThroughAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IMemoryCache cache)
        : base(options, logger, encoder)
    {
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for Authorization header presence
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        var authHeader = authHeaderValue.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
             return AuthenticateResult.Fail("Empty Authorization Header");
        }

        // Validate Scheme
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid Authentication Scheme");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Check cache
        string cacheKey = $"auth_token_{token}";
        if (_cache.TryGetValue(cacheKey, out bool isValid) && isValid)
        {
            return CreateSuccessResult();
        }

        try
        {
            var userApi = Context.RequestServices.GetRequiredService<IUserApi>();
            var response = await userApi.GetAsync();

            if (response.Result)
            {
                // Cache successful validation for 5 minutes
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                return CreateSuccessResult();
            }

            return AuthenticateResult.Fail("Invalid Token");
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return AuthenticateResult.Fail("Unauthorized: Invalid Token");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating token with Raindrop API");
            return AuthenticateResult.Fail("Authentication Error");
        }
    }

    private AuthenticateResult CreateSuccessResult()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "RaindropUser") };
        var identity = new ClaimsIdentity(claims, "PassThrough");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
