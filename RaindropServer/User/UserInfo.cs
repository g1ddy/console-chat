using System.Text.Json.Serialization;

namespace RaindropServer.User;

public record UserInfo
{
    [JsonPropertyName("_id")]
    public int Id { get; init; }

    public string? Email { get; init; }

    public string? FullName { get; init; }

    public bool Pro { get; init; }

    public object? Config { get; init; }

    public object? Dropbox { get; init; }

    public object? Files { get; init; }

    public string? Type { get; init; }
}
