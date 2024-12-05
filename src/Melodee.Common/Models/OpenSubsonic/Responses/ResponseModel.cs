using System.Text.Json.Serialization;

namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ResponseModel
{
    public bool IsSuccess { get; init; } = true;

    public required UserInfo UserInfo { get; init; }

    /// <summary>
    ///     This is the '"subsonic-response"' level object returned to API consumers.
    /// </summary>
    [JsonPropertyName("subsonic-response")]
    public required ApiResponse ResponseData { get; init; }

    public long TotalCount { get; init; }
}
