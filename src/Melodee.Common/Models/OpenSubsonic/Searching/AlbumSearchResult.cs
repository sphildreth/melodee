using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic.Searching;

/// <summary>
///     This is returned in search results
///     <remarks>
///         see  https://www.subsonic.org/pages/inc/api/examples/searchResult3_example_1.xml
///     </remarks>
/// </summary>
public record AlbumSearchResult : IOpenSubsonicToXml
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string CoverArt { get; init; }

    public short SongCount { get; init; }

    public required string Artist { get; init; }

    public required string ArtistId { get; init; }

    [JsonIgnore] public Instant CreatedAt { get; init; }

    [JsonIgnore] public double DurationMs { get; init; }

    public int Duration => DurationMs.ToSeconds();

    public string Created => CreatedAt.ToString();

    public string[]? Genres { get; init; }

    public string ToXml(string? nodeName = null)
    {
        return $"<album id=\"{Id}\" name=\"{Name}\" coverArt=\"{CoverArt}\" songCount=\"{SongCount}\" created=\"{Created}\" duration=\"{Duration}\" artist=\"{Artist}\" artistId=\"{ArtistId}\"/>";
    }
}
