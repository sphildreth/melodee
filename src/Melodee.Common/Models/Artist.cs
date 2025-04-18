using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Models;

public record Artist(
    string Name,
    string NameNormalized,
    string? SortName,
    IEnumerable<ImageInfo>? Images = null,
    int? ArtistDbId = null)
{
    public const string JsonFileName = "artist.melodee.json";

    /// <summary>
    ///     All Music Guide Artist Id
    /// </summary>
    public string? AmgId { get; set; }

    public string? ItunesId { get; set; }

    public string? DiscogsId { get; set; }

    public string? WikiDataId { get; set; }

    public Guid? MusicBrainzId { get; set; }

    public string? LastFmId { get; set; }

    public string? SpotifyId { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();

    public long? SearchEngineResultUniqueId { get; set; }

    public string? OriginalName { get; init; }

    public static Artist NewArtistFromName(string name)
    {
        return new Artist(name, name.ToNormalizedString() ?? name, name.ToTitleCase());
    }

    public Artist Merge(Artist other)
    {
        var images = new List<ImageInfo>(Images ?? []);
        if (other.Images != null)
        {
            foreach (var image in other.Images)
            {
                if (!images.Any(x => x.IsCrcHashMatch(image.CrcHash)))
                {
                    images.Add(image);
                }
            }
        }

        return new Artist(Name ?? other.Name, NameNormalized ?? other.NameNormalized, SortName ?? other.SortName, images, ArtistDbId ?? other.ArtistDbId)
        {
            AmgId = AmgId ?? other.AmgId,
            DiscogsId = DiscogsId ?? other.DiscogsId,
            ItunesId = ItunesId ?? other.ItunesId,
            LastFmId = LastFmId ?? other.LastFmId,
            MusicBrainzId = MusicBrainzId ?? other.MusicBrainzId,
            SearchEngineResultUniqueId = SearchEngineResultUniqueId ?? other.SearchEngineResultUniqueId,
            SpotifyId = SpotifyId ?? other.SpotifyId,
            WikiDataId = WikiDataId ?? other.WikiDataId
        };
    }
}
