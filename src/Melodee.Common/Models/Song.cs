using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

public sealed record Song
{
    public Guid? ArtistId { get; set; }

    public Guid? AlbumId { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();

    public required string CrcHash { get; init; }

    public long DuplicateHashCheck =>
        SafeParser.Hash(this.AlbumTitle(), this.SongNumber().ToString(), this.Title().ToNormalizedString());

    public required FileSystemFileInfo File { get; init; }

    [JsonIgnore] public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; init; }

    public IEnumerable<MediaAudio<object?>>? MediaAudios { get; init; }

    public int SortOrder { get; set; }

    public string DisplaySummary =>
        $"{this.SongNumber().ToStringPadLeft(3)}/{this.SongTotalNumber().ToStringPadLeft(3)} : {this.Title()}";

    public override string ToString()
    {
        return $"ArtistId [{ArtistId}] AlbumId [{AlbumId}] SongId [{Id}] File [{File}]";
    }

    public static Song IdentityBestAndMergeOthers(Song[] songs)
    {
        if (songs.Length == 1)
        {
            return songs[0];
        }

        var best = songs[0];
        foreach (var song in songs.Skip(1))
        {
            if (song.Duration() >= best.Duration() || song.BitRate() > best.BitRate() ||
                song.BitDepth() > best.BitDepth())
            {
                var tags = (best.Tags ?? []).ToList();
                foreach (var tagItem in song.Tags ?? [])
                {
                    if (tags.FirstOrDefault(x => x.Identifier == tagItem.Identifier) == null)
                    {
                        tags.Add(tagItem);
                    }
                }

                best = song with
                {
                    Tags = tags
                };
            }
        }

        return best;
    }
}
