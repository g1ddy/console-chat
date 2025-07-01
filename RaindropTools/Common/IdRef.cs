using System.Text.Json.Serialization;

namespace RaindropTools.Common;

public class IdRef
{
    [JsonPropertyName("$id")]
    public int Id { get; set; }
}
