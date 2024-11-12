using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Plugins.MetaData.Directory.Models;

public sealed record CueSheet
{
    public bool IsValid => !string.IsNullOrWhiteSpace(MediaFileSystemFileInfo.Name) && 
                           Songs.Any() && 
                           Tags.Any() && 
                           Tags.Count(x => x.Identifier == MetaTagIdentifier.Album) == 1 &&
                           Tags.Count(x => x.Identifier == MetaTagIdentifier.AlbumArtist) == 1 &&
                           Tags.Count(x => x.Identifier == MetaTagIdentifier.OrigAlbumYear) == 1 &&
                           SongIndexes.Any() && 
                           MediaFileSystemFileInfo.Exists(FileSystemDirectoryInfo);

    /// <summary>
    /// This is the media file that is to be split up for the CUE file.
    /// </summary>
    public required FileSystemFileInfo MediaFileSystemFileInfo { get; init; }

    public required FileSystemDirectoryInfo FileSystemDirectoryInfo { get; init; }

    public required IEnumerable<Common.Models.Song> Songs { get; init; }

    public required IEnumerable<CueIndex> SongIndexes { get; init; }

    public required IEnumerable<MetaTag<object?>> Tags { get; init; }
}
