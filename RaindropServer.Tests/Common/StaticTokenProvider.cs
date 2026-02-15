using RaindropServer.Common;

namespace RaindropServer.Tests.Common;

/// <summary>
/// Provides a static token for testing.
/// </summary>
public class StaticTokenProvider : ITokenProvider
{
    private readonly string? _token;

    public StaticTokenProvider(string? token)
    {
        _token = token;
    }

    public string? GetToken()
    {
        return _token;
    }
}
