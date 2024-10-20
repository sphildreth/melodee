using System.Text.Json.Serialization;
using Melodee.Common.Enums;

namespace Melodee.Common.Models;

[Serializable]
public sealed record ImageInfo
{
    /// <summary>
    ///     May not exist if the image was extracted from tag.
    /// </summary>
    public FileSystemFileInfo? FileInfo { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PictureIdentifier PictureIdentifier { get; init; }

    public required string CrcHash { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public int SortOrder { get; init; }

    public bool WasEmbeddedInSong { get; init; }

    public override string ToString()
    {
        return $"CrcHash [{CrcHash}] [{PictureIdentifier}] Width [{Width}] Height: [{Height}] FileInfo [{FileInfo}]";
    }
}
