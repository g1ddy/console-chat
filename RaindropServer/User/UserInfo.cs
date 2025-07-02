using System.Text.Json.Serialization;

namespace RaindropTools.User;

public class UserInfo
{
    [JsonPropertyName("_id")]
    public int Id { get; set; }

    public string? Email { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    public bool Pro { get; set; }

    public object? Config { get; set; }

    public object? Dropbox { get; set; }

    public object? Files { get; set; }

    public string? Type { get; set; }
}
