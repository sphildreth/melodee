
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SearchEngines;

/// <summary>
/// A result for an artist search engine search
/// </summary>
/// <param name="FromPlugin">Name of Plugin who returned result.</param>
/// <param name="UniqueId">UniqueId for the result</param>
/// <param name="Rank">Ranked, higher number the better quality of the result to the query.</param>
/// <param name="Name">Artist name</param>
/// <param name="ApiKey">Artist ApiKey (if found in database)</param>
/// <param name="SortName">Artist sort name</param>
/// <param name="RealName">Artist real name</param>
/// <param name="ImageUrl">Public URL to an image.</param>
/// <param name="MusicBrainzId">Any found MusicBrainzId</param>
/// <param name="Releases">Collection of releases for artist</param>
public record ArtistSearchResult(
    string FromPlugin,
    long UniqueId,
    int Rank,
    string Name,
    Guid? ApiKey = null,
    string? SortName = null,
    string? RealName = null,
    string? ImageUrl=null,
    string? MusicBrainzId = null,
    AlbumSearchResult[]? Releases = null);
