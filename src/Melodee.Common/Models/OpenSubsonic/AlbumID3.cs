using System.Text;

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
    
    /// <summary></summary>
    public string? CoverArt { get; init; }
    
    /// <summary></summary>
    public short SongCount { get; init; }    
    
    /// <summary>
    ///     Total duration of the album in seconds
    /// </summary>
    public required int Duration { get; init; }    
    
    /// <summary></summary>
    public int? PlayCount { get; init; }    
    
    /// <summary>
    ///     Date the album was added. [ISO 8601]
    /// </summary>
    public required string Created { get; init; }  
    
    /// <summary></summary>
    public string? Starred { get; init; }    
    
    /// <summary></summary>
    public int? Year { get; init; }    

    /// <summary></summary>
    public string? Genre { get; init; }

    public string ToXml(string? nodeName = null)
    {
        string starredAttribute = string.Empty;
        if (Starred != null)
        {
            starredAttribute = $" starred=\"{Starred}\"";
        }            
        return $"<album id=\"{ Id }\" name=\"{ Name }\" coverArt=\"{ CoverArt }\" songCount=\"{ SongCount }\" " +
                                       $"playCount=\"{ PlayCount }\" year=\"{ Year }\" genre=\"{ Genre }\"{ starredAttribute } " +
                                       $"created=\"{ Created }\" duration=\"{ Duration }\" artist=\"{ Artist }\" artistId=\"{ ArtistId }\"/>";
    }
}
