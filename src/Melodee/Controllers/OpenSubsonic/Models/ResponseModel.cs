using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace Melodee.Controllers.OpenSubsonic.Models;

public record ResponseModel<T>
{
    [JsonPropertyName("subsonic-response")]
    public required T ResponseData { get; init; }
};
