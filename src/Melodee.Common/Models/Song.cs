using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Models;

public sealed record Song
{
    public Guid? ArtistId { get; set; }

    public Guid? AlbumId { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();

    public required string CrcHash { get; init; }

    public required FileSystemFileInfo File { get; init; }

    [JsonIgnore] public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; init; }

    public IEnumerable<MediaAudio<object?>>? MediaAudios { get; init; }

    public int SortOrder { get; set; }

    public string DisplaySummary => $"{this.MediaNumber().ToStringPadLeft(2)}/{this.MediaTotalNumber().ToStringPadLeft(2)} : {this.SongNumber().ToStringPadLeft(3)}/{this.SongTotalNumber().ToStringPadLeft(3)} : {this.Title()}";

    public override string ToString()
    {
        return $"ArtistId [{ArtistId}] AlbumId [{AlbumId}] SongId [{Id}] File [{File}]";
    }
}
