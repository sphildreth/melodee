namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
/// A media.
/// </summary>
/// <param name="Id"></param>
/// <param name="Parent"></param>
/// <param name="IsDir"></param>
/// <param name="Title"></param>
/// <param name="Album"></param>
/// <param name="Artist"></param>
/// <param name="Track"></param>
/// <param name="Year"></param>
/// <param name="CoverArt"></param>
/// <param name="Size"></param>
/// <param name="ContentType"></param>
/// <param name="Suffix"></param>
/// <param name="Starred"></param>
/// <param name="Duration"></param>
/// <param name="BitRate"></param>
/// <param name="BitDepth"></param>
/// <param name="SamplingRate"></param>
/// <param name="ChannelCount"></param>
/// <param name="Path"></param>
/// <param name="PlayCount"></param>
/// <param name="Played"></param>
/// <param name="DiscNumber"></param>
/// <param name="Created"></param>
/// <param name="AlbumId"></param>
/// <param name="ArtistId"></param>
/// <param name="Type"></param>
/// <param name="MediaType"></param>
/// <param name="IsVideo"></param>
/// <param name="Bpm"></param>
/// <param name="Comment"></param>
/// <param name="SortName"></param>
/// <param name="MusicBrainzId"></param>
/// <param name="Genres"></param>
/// <param name="Artists"></param>
/// <param name="DisplayArtist"></param>
/// <param name="AlbumArtists"></param>
/// <param name="DisplayAlbumArtist"></param>
/// <param name="Contributors"></param>
/// <param name="DisplayComposer"></param>
/// <param name="Moods"></param>
/// <param name="ReplayGain"></param>
public record Child(
    string Id,
    string? Parent,
    bool? IsDir,
    string?Title,
    string? Album,
    string? Artist,
    int? Track,
    int? Year,
    string? CoverArt,
    int? Size,
    string? ContentType,
    string? Suffix,
    string? Starred,
    int? Duration,
    int? BitRate,
    int? BitDepth,
    int? SamplingRate,
    int? ChannelCount,
    string? Path,
    int? PlayCount,
    string? Played,
    int? DiscNumber,
    string? Created,
    string? AlbumId,
    string? ArtistId,
    string? Type,
    string? MediaType,
    bool? IsVideo,
    int? Bpm,
    string? Comment,
    string? SortName,
    string? MusicBrainzId,
    Genre[]? Genres,
    Artist[]? Artists,
    string? DisplayArtist,
    Artist[]? AlbumArtists,
    string? DisplayAlbumArtist,
    Contributor[]? Contributors,
    string? DisplayComposer,
    string[]? Moods,
    ReplayGain? ReplayGain
);
