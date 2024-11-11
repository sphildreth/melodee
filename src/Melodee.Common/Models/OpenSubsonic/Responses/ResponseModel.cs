using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ResponseModel<T>
{
    [JsonPropertyName("subsonic-response")]
    public required T ResponseData { get; init; }
};
