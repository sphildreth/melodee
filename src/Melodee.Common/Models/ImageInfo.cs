using System.Text.Json.Serialization;
using Melodee.Common.Enums;

namespace Melodee.Common.Models;

public sealed record ImageInfo
{
    /// <summary>
    ///     This is used so the images sort proper in the directory not mixed with the song names.
    /// </summary>
    public const string ImageFilePrefix = "i-";

    /// <summary>
    ///     May not exist if album image and the image was extracted from tag.
    /// </summary>
    public FileSystemFileInfo? FileInfo { get; init; }
    
    /// <summary>
    /// This is populated when the image is in a subdirectory of the album otherwise is null.
    /// </summary>
    public FileSystemDirectoryInfo? DirectoryInfo { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PictureIdentifier PictureIdentifier { get; init; }

    public required string CrcHash { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public int SortOrder { get; init; }

    public bool WasEmbeddedInSong { get; init; }

    public string? OriginalFilename { get; init; }

    public override string ToString()
    {
        return $"CrcHash [{CrcHash}] [{PictureIdentifier}] Width [{Width}] Height: [{Height}] FileInfo [{FileInfo}]";
    }
}
