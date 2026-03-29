using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaindropServer.User;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RaindropServer.Common;

public class PassThroughAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string CacheKeyPrefix = "TokenValidation_";
    private static readonly TimeSpan SuccessfulValidationCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan FailedValidationCacheDuration = TimeSpan.FromMinutes(1);

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

        // Validate Token Format (GUID)
        if (!Guid.TryParse(token, out _))
        {
            return AuthenticateResult.Fail("Invalid Token Format");
        }

        // Check Cache
        string cacheKey = $"{CacheKeyPrefix}{token}";
        if (_cache.TryGetValue(cacheKey, out bool isValid))
        {
            return isValid ? Success() : AuthenticateResult.Fail("Invalid Token");
        }

        // Validate Token with Raindrop API
        try
        {
            var userApi = Context.RequestServices.GetRequiredService<IUserApi>();
            // The AuthHeaderHandler will automatically pick up the token from HttpContext
            // but since we are in the authentication handler, the identity is not set yet.
            // However, AuthHeaderHandler uses ITokenProvider which uses HttpContext.Request.Headers["Authorization"]
            // which is already present.
            var response = await userApi.GetAsync();

            if (response.Result)
            {
                _cache.Set(cacheKey, true, SuccessfulValidationCacheDuration);
                return Success();
            }
            else
            {
                _cache.Set(cacheKey, false, FailedValidationCacheDuration);
                return AuthenticateResult.Fail("Invalid Token");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating token with Raindrop API");
            return AuthenticateResult.Fail("Token validation failed");
        }

        AuthenticateResult Success()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "RaindropUser") };
            var identity = new ClaimsIdentity(claims, "PassThrough");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
