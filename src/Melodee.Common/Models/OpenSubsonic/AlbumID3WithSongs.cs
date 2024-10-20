namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
/// Album with songs.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Artist"></param>
/// <param name="Year"></param>
/// <param name="CoverArt"></param>
/// <param name="Starred"></param>
/// <param name="Duration"></param>
/// <param name="PlayCount"></param>
/// <param name="Genre"></param>
/// <param name="Created"></param>
/// <param name="ArtistId"></param>
/// <param name="SongCount"></param>
/// <param name="Played"></param>
/// <param name="UserRating"></param>
/// <param name="RecordLabels"></param>
/// <param name="MusicBrainzId"></param>
/// <param name="Genres"></param>
/// <param name="Artists"></param>
/// <param name="DisplayArtist"></param>
/// <param name="AlbumTypes"></param>
/// <param name="Moods"></param>
/// <param name="SortName"></param>
/// <param name="OriginalAlbumDate"></param>
/// <param name="AlbumDate"></param>
/// <param name="IsCompilation"></param>
/// <param name="DiscTitles"></param>
/// <param name="Song"></param>
public record AlbumId3WithSongs(
    string Id,
    string Name,
    string? Artist,
    int? Year,
    string? CoverArt,
    string? Starred,
    int Duration,
    int? PlayCount,
    string? Genre,
    string Created,
    string? ArtistId,
    int SongCount,
    string? Played,
    int? UserRating,
    RecordLabel[]? RecordLabels,
    string? MusicBrainzId,
    Genre[]? Genres,
    Artist[]? Artists,
    string? DisplayArtist,
    string[]? AlbumTypes,
    string[]? Moods,
    string? SortName,
    ItemDate? OriginalAlbumDate,
    ItemDate? AlbumDate,
    bool? IsCompilation,
    DiscTitle[]? DiscTitles,
    Child[]? Song
);
