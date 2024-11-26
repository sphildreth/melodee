using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SearchEngines;

public record ArtistQuery(string Name, string[]? AlbumNames, string? MusicBrainzId)
{
    public string NameNormalized => Name.ToNormalizedString() ?? Name;
}
