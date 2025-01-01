using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Plugins.MetaData.Directory.Models;

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

    public string[]? ValidationMessages
    {
        get
        {
            var result = new List<string>();
            if (!Songs.Any())
            {
                result.Add("No Songs Found");
            }

            if (!Tags.Any())
            {
                result.Add("No Tags Found");
            }

            if (Tags.Count(x => x.Identifier == MetaTagIdentifier.Album) != 1)
            {
                result.Add("Album Tag Not Found");
            }

            if (Tags.Count(x => x.Identifier == MetaTagIdentifier.AlbumArtist) != 1)
            {
                result.Add("Album Artist Tag Not Found");
            }

            if (Tags.Count(x => x.Identifier == MetaTagIdentifier.OrigAlbumYear) != 1)
            {
                result.Add("Album Year Tag Not Found");
            }

            if (!SongIndexes.Any())
            {
                result.Add("No Song Indexes Found");
            }

            if (!MediaFileSystemFileInfo.Exists(FileSystemDirectoryInfo))
            {
                result.Add($"Media File [{MediaFileSystemFileInfo}] Not Found");
            }

            return result.ToArray();
        }
    }

    /// <summary>
    ///     This is the media file that is to be split up for the CUE file.
    /// </summary>
    public required FileSystemFileInfo MediaFileSystemFileInfo { get; init; }

    public required FileSystemDirectoryInfo FileSystemDirectoryInfo { get; init; }

    public required IEnumerable<Common.Models.Song> Songs { get; init; }

    public required IEnumerable<CueIndex> SongIndexes { get; init; }

    public required IEnumerable<MetaTag<object?>> Tags { get; init; }
}
