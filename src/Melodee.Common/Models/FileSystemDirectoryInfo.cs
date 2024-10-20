using System.Text.Json.Serialization;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
///     This is a representation of a "file" directory.
/// </summary>
[Serializable]
public sealed record FileSystemDirectoryInfo
{
    public long UniqueId => SafeParser.Hash(Path);

    [JsonIgnore] public bool ShowInTree => MusicFilesFound > 0;

    public long ParentId { get; init; }

    /// <summary>
    ///     Full path to Directory
    /// </summary>
    public required string Path { get; init; }

    public required string Name { get; init; }

    /// <summary>
    ///     This is the total number of items found when the Album was initially processed, does not necessarily include any
    ///     files extracted or converted during processing.
    /// </summary>
    public int? TotalItemsFound { get; init; }

    public int? MusicFilesFound { get; init; }

    public int? MusicMetaDataFilesFound { get; init; }

    /// <summary>
    ///     This is the count of images found in the Path directory at the time of initial processing, does not necessarily
    ///     include any extract images from Songs.
    /// </summary>
    public int? ImageFilesFound { get; init; }

    public override string ToString()
    {
        return $"UniqueId [{UniqueId}] Path [{Path}] ShortName [{Name}]";
    }

    public static FileSystemDirectoryInfo Blank()
    {
        return new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        };
    }
}
