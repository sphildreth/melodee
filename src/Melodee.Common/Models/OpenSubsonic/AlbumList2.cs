using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic;

public record AlbumList2 : IOpenSubsonicToXml
{
    public required string Id { get; init; }

    public required string Album { get; init; }

    public required string Title { get; init; }

    public required string Name { get; init; }

    public required string CoverArt { get; init; }

    public required int SongCount { get; init; }

    [JsonIgnore] public Instant? CreatedRaw { get; init; }

    public string Created => CreatedRaw?.ToString() ?? string.Empty;

    //public string Created => "2024-10-14T12:12:14.692182698-05:00";

    public required int Duration { get; init; }

    public required int PlayedCount { get; init; }

    public required string ArtistId { get; init; }

    public required string Artist { get; init; }

    public required int Year { get; init; }

    public int? UserRating { get; init; }

    public string[]? Genres { get; init; }

    public string? Genre => Genres?.Length > 0 ? Genres[0] : null;

    /// <summary>
    ///     ApiKeyId of Library
    /// </summary>
    public required string Parent { get; set; }

    public Guid? MusicBrainzId { get; init; }

    public string? Comment { get; init; }

    public string? SortName { get; init; }

    public virtual string ToXml(string? nodeName = null)
    {
        return $"<album id=\"{Id}\" parent=\"{Parent}\" isDir=\"true\" title=\"{Name.ToSafeXmlString()}\" " +
               $"name=\"{Name.ToSafeXmlString()}\" album=\"{Name.ToSafeXmlString()}\" artist=\"{Artist.ToSafeXmlString()}\" year=\"{Year}\" genre=\"{Genre.ToSafeXmlString()}\" " +
               $"coverArt=\"{CoverArt}\" duration=\"{Duration}\" " +
               $"created=\"{Created}\" artistId=\"{ArtistId}\" " +
               $"songCount=\"{SongCount}\" isVideo=\"false\" bpm=\"0\" comment=\"{Comment.ToSafeXmlString()}\" sortName=\"{SortName.ToSafeXmlString()}\" mediaType=\"album\" " +
               $"musicBrainzId=\"{MusicBrainzId}\" channelCount=\"0\" samplingRate=\"0\"><replayGain></replayGain></album>";
    }
}
