using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.OpenSubsonic;

public record AlbumID3 : IOpenSubsonicToXml
{
    /// <summary>
    ///     The id of the album
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     The album name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The id of the artist
    /// </summary>
    public string? Artist { get; init; }
    
    /// <summary></summary>
    public string? ArtistId { get; init; }
    
    public string? CoverArt { get; init; }
    
    public short SongCount { get; init; }    
    
    /// <summary>
    ///     Total duration of the album in seconds
    /// </summary>
    public required int Duration { get; init; }    
    
    public int? PlayCount { get; init; }  
   
    [JsonIgnore] public Instant? CreatedRaw { get; init; }

    /// <summary>
    ///     Date the album was added. [ISO 8601]
    /// </summary>
    public string Created => CreatedRaw?.ToString() ?? string.Empty;    
    
    public string? Starred { get; init; }    
    
    public int? Year { get; init; }    

    public string[]? Genres { get; init; }
    
    public string? Genre => Genres?.Length > 0 ? Genres[0] : null;

    public string ToXml(string? nodeName = null)
    {
        string starredAttribute = string.Empty;
        if (Starred != null)
        {
            starredAttribute = $" starred=\"{Starred}\"";
        }
        string genreAttribute = string.Empty;
        if (Genre != null)
        {
            genreAttribute = $" genre=\"{Genre.ToSafeXmlString()}\"";
        }            
        
        return $"<album id=\"{ Id }\" name=\"{ Name.ToSafeXmlString() }\" coverArt=\"{ CoverArt }\" songCount=\"{ SongCount }\" " +
                                       $"playCount=\"{ PlayCount }\" year=\"{ Year }\"{genreAttribute}{ starredAttribute } " +
                                       $"created=\"{ Created }\" duration=\"{ Duration }\" artist=\"{ Artist.ToSafeXmlString() }\" artistId=\"{ ArtistId }\"></album>";
    }
}
