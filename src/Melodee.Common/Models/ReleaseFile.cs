using System.Text.Json.Serialization;
using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record ReleaseFile
{
    public long ReleaseUniqueId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ReleaseFileType ReleaseFileType { get; init; }

    public required string ProcessedByPlugin { get; init; }

    public required FileSystemFileInfo FileSystemFileInfo { get; init; }
}
