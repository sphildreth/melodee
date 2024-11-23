namespace Melodee.Common.Models.OpenSubsonic.Searching;

/// <summary>
/// This is returned in search results
/// <remarks>
/// see  https://www.subsonic.org/pages/inc/api/examples/searchResult3_example_1.xml
/// </remarks>
/// </summary>
/// <param name="Id">The id of the album</param>
/// <param name="Name">The album name.</param>
/// <param name="CoverArt">A covertArt id.</param>
/// <param name="SongCount">Number of songs</param>
/// <param name="Created">Created date in ISO8601 format</param>
/// <param name="Duration">Duration in seconds</param>
/// <param name="Artist">The artist name.</param>
/// <param name="ArtistId">The id of the artist</param>
public record AlbumSearchResult
(
    string Id, 
    string Name, 
    string CoverArt,
    int SongCount,
    string Created,
    int Duration,
    string Artist,
    string ArtistId);
