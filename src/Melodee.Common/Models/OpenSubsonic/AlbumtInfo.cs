namespace Melodee.Common.Models.OpenSubsonic;

public record AlbumInfo(
    string Id,
    string Name,
    string? SmallImageUrl = null,
    string? MediumImageUrl = null,
    string? LargeImageUrl = null,
    int? SongCount = null,
    int? AlbumCount = null,
    string? notes  = null,
    Guid? MusicBrainzArtistId = null) : NamedInfo(Id,
    Name,
    SmallImageUrl,
    MediumImageUrl,
    LargeImageUrl,
    SongCount,
    AlbumCount);
