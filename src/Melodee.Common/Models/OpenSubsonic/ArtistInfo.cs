namespace Melodee.Common.Models.OpenSubsonic;

public record ArtistInfo(
    string Id,
    string Name,
    string? SmallImageUrl = null,
    string? MediumImageUrl = null,
    string? LargeImageUrl = null,
    int? SongCount = null,
    int? AlbumCount = null,
    string? Biography = null,
    Guid? MusicBrainzArtistId = null,
    Artist[]? SimilarArtist = null) : NamedInfo(Id,
    Name,
    SmallImageUrl,
    MediumImageUrl,
    LargeImageUrl,
    SongCount,
    AlbumCount);
