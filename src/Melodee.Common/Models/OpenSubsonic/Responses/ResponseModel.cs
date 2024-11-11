using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ResponseModel<T>
{
    [JsonIgnore] public bool IsSuccess { get; init; }

    [JsonPropertyName("subsonic-response")]
    public required T ResponseData { get; init; }
}
