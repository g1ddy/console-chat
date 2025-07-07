using System.Text.Json.Serialization;

namespace RaindropServer.User;

public class UserInfo
{
    [JsonPropertyName("_id")]
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public bool Pro { get; set; }

    public object? Config { get; set; }

    public object? Dropbox { get; set; }

    public object? Files { get; set; }

    public string? Type { get; set; }
}
