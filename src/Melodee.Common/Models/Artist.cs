using Melodee.Common.Extensions;

namespace Melodee.Common.Models;

public record Artist(
    string Name,
    string NameNormalized,
    string? SortName,
    IEnumerable<ImageInfo>? Images = null,
    string? MusicBrainzId = null)
{
    public static Artist NewArtistFromName(string name) 
        => new Artist(name, name.ToNormalizedString() ?? name, name.ToTitleCase());
}
