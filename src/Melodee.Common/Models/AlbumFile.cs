using System.Text.Json.Serialization;
using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record AlbumFile
{
    public long AlbumUniqueId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AlbumFileType AlbumFileType { get; init; }

    public required string ProcessedByPlugin { get; init; }

    public required FileSystemFileInfo FileSystemFileInfo { get; init; }
}
