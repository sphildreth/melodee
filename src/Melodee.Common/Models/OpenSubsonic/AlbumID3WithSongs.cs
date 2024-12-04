using System.Text;

namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     Album with songs.
///     <remarks>https://opensubsonic.netlify.app/docs/responses/albumid3/</remarks>
/// </summary>
public record AlbumId3WithSongs : IOpenSubsonicToXml
{
    /// <summary>
    ///     The id of the album
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     The album name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary></summary>
    public string? Title { get; init; }

    /// <summary>
    ///     The id of the artist
    /// </summary>
    public string? Artist { get; init; }

    /// <summary></summary>
    public int? Year { get; init; }

    /// <summary></summary>
    public string? CoverArt { get; init; }

    /// <summary></summary>
    public string? Starred { get; init; }

    /// <summary>
    ///     Total duration of the album in seconds
    /// </summary>
    public required int Duration { get; init; }

    /// <summary></summary>
    public int? PlayCount { get; init; }

    /// <summary></summary>
    public string? Genre { get; init; }

    /// <summary>
    ///     Date the album was added. [ISO 8601]
    /// </summary>
    public required string Created { get; init; }

    /// <summary></summary>
    public string? ArtistId { get; init; }

    /// <summary></summary>
    public short SongCount { get; init; }

    /// <summary></summary>
    public string? Played { get; init; }

    /// <summary></summary>
    public int? UserRating { get; init; }

    /// <summary></summary>
    public RecordLabel[]? RecordLabels { get; init; }

    /// <summary></summary>
    public string? MusicBrainzId { get; init; }

    /// <summary></summary>
    public Genre[]? Genres { get; init; }

    /// <summary></summary>
    public ArtistID3[]? Artists { get; init; }

    /// <summary></summary>
    public string? DisplayArtist { get; init; }

    /// <summary></summary>
    public string[]? AlbumTypes { get; init; }

    /// <summary></summary>
    public string[]? Moods { get; init; }

    /// <summary></summary>
    public string? SortName { get; init; }

    /// <summary>Date the album was originally released.</summary>
    public ItemDate? OriginalAlbumDate { get; init; }

    /// <summary>
    ///     Date the specific edition of the album was released. Note: for files using ID3 tags, releaseDate should
    ///     generally be read from the TDRL tag. Servers that use a different source for this field should document the
    ///     behavior.
    /// </summary>
    public ItemDate? AlbumDate { get; init; }

    /// <summary></summary>
    public bool? IsCompilation { get; init; }

    /// <summary></summary>
    public DiscTitle[]? DiscTitles { get; init; }

    /// <summary></summary>
    public Child[]? Song { get; init; }

    public required string Parent { get; set; }
    public string ToXml(string? nodeName = null)
    {
        var result = new StringBuilder($"<album id=\"{ Id }\" name=\"{ Name }\" coverArt=\"{ CoverArt }\" songCount=\"{ SongCount }\" created=\"{ Created }\" duration=\"{ Duration }\" artist=\"{ Name }\" artistId=\"{ ArtistId }\">");

    
        result.Append("</album>");
        return result.ToString();

    }
}
