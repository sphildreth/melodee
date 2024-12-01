using System.Text.Json.Serialization;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic.Searching;

public record SongSearchResult
{
    public string Type { get; } = "music";

    public bool IsDir { get; } = false;

    public required string Id { get; init; }

    public required string Parent { get; init; }

    public required string Title { get; init; }

    public required string Album { get; init; }

    public required string Artist { get; init; }

    public required string CoverArt { get; init; }

    [JsonIgnore] public Instant CreatedAt { get; init; }

    public string Created => CreatedAt.ToString();

    public int Duration { get; init; }

    public int Bitrate { get; init; }

    public int Track { get; init; }

    public int Year { get; init; }

    public string[]? Genres { get; init; }

    public required string Suffix { get; init; }

    public required string ContentType { get; init; }

    public required string Path { get; init; }

    public required string AlbumId { get; init; }

    public required string ArtistId { get; init; }
}
