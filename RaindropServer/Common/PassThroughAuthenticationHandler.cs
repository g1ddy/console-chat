using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RaindropServer.Common;

public class PassThroughAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public PassThroughAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for Authorization header presence
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        var authHeader = authHeaderValue.ToString();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
             return Task.FromResult(AuthenticateResult.Fail("Empty Authorization Header"));
        }

        // Validate Scheme
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authentication Scheme"));
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Validate Token Format (GUID)
        if (!Guid.TryParse(token, out _))
        {
             return Task.FromResult(AuthenticateResult.Fail("Invalid Token Format"));
        }

        // Create a user identity.
        var claims = new[] { new Claim(ClaimTypes.Name, "RaindropUser") };
        var identity = new ClaimsIdentity(claims, "PassThrough");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
