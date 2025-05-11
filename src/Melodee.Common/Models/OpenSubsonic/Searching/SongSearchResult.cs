using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic.Searching;

public record SongSearchResult : IOpenSubsonicToXml
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

    public int Size { get; init; }

    public int Bitrate { get; init; }

    public int Track { get; init; }

    public int Year { get; init; }

    public string[]? Genres { get; init; }

    public required string Suffix { get; init; }

    public required string ContentType { get; init; }

    public required string Path { get; init; }

    public required string AlbumId { get; init; }

    public required string ArtistId { get; init; }

    public string Genre => string.Join(",", Genres ?? []);

    public string ToXml(string? nodeName = null)
    {
        return
            $"<song id=\"{Id}\" parent=\"{Parent}\" title=\"{Title.ToSafeXmlString()}\" album=\"{Album.ToSafeXmlString()}\" " +
            $"artist=\"{Artist.ToSafeXmlString()}\" isDir=\"{IsDir.ToLowerCaseString()}\" coverArt=\"{CoverArt}\" created=\"{Created}\" " +
            $"duration=\"{Duration}\" bitRate=\"{Bitrate}\" track=\"{Track}\" genre=\"({Genre.ToSafeXmlString()}\" size=\"{Size}\" suffix=\"{Suffix}\" " +
            $"contentType=\"{ContentType}\" isVideo=\"false\" path=\"{Path}\" " +
            $"albumId=\"{AlbumId}\" artistId=\"{ArtistId}\" type=\"{Type}\"/>";
    }
}
