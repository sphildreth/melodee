using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

public record Artist(
    string Name,
    string NameNormalized,
    string? SortName,
    IEnumerable<ImageInfo>? Images = null,
    int? ArtistDbId = null,
    string? MusicBrainzId = null)
{
    
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public long? SearchEngineResultUniqueId { get; init; }

    public string? OriginalName { get; init; }

    public static Artist NewArtistFromName(string name) 
        => new Artist(name, name.ToNormalizedString() ?? name, name.ToTitleCase());
}
