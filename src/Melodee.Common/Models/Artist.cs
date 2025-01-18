using Melodee.Common.Extensions;

namespace Melodee.Common.Models;

public record Artist(
    string Name,
    string NameNormalized,
    string? SortName,
    IEnumerable<ImageInfo>? Images = null,
    int? ArtistDbId = null)
{
    /// <summary>
    /// All Music Guide Artist Id
    /// </summary>
    public string? AmgId { get; set; }

    public string? ItunesId { get; set; }
    
    public string? DiscogsId { get; set; }

    public string? WikiDataId { get; set; }
    
    public Guid? MusicBrainzId { get; set; }
    
    public string? LastFmId { get; set; }

    public string? SpotifyId { get; set; }  
    
    public Guid Id { get; set; } = Guid.NewGuid();

    public long? SearchEngineResultUniqueId { get; init; }

    public string? OriginalName { get; init; }

    public static Artist NewArtistFromName(string name)
    {
        return new Artist(name, name.ToNormalizedString() ?? name, name.ToTitleCase());
    }
}
