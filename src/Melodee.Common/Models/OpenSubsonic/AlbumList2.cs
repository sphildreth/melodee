using System.Text.Json.Serialization;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic;

public sealed record AlbumList2
{
    public required string Id { get; init; }

    public required string Album { get; init; }

    public required string Title { get; init; }

    public required string Name { get; init; }

    public required string CoverArt { get; init; }

    public required int SongCount { get; init; }

    [JsonIgnore] public Instant? CreatedRaw { get; init; }

    public string Created => CreatedRaw?.ToString() ?? string.Empty;

    public required int Duration { get; init; }

    public required int PlayedCount { get; init; }

    public required string ArtistId { get; init; }

    public required string Artist { get; init; }

    public required int Year { get; init; }

    public int? UserStarredCount { get; init; }

    public bool Starred { get; init; }

    public int? UserRating { get; init; }

    public string? Genre { get; init; }

    public string[]? Genres => Genre?.Split('|').Select(x => x.Trim()).ToArray();
}
