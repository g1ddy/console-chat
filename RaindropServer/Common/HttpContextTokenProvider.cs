namespace RaindropServer.Common;

/// <summary>
/// Retrieves the token from the current HTTP request context.
/// </summary>
public class HttpContextTokenProvider : ITokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetToken()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring("Bearer ".Length).Trim();
            }

            // Other authentication schemes are not supported for pass-through.
            return null;
        }

        return null;
    }
}
