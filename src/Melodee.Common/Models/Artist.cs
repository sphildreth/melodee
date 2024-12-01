using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

public record Artist(
    string Name,
    string NameNormalized,
    string? SortName,
    IEnumerable<ImageInfo>? Images = null,
    string? MusicBrainzId = null)
{
    
    public long? SearchEngineResultUniqueId { get; init; }
    
    public string? OriginalName { get; init; }
   
    public static long GenerateUniqueId(string? musicBrainzId, string name) => SafeParser.Hash(musicBrainzId ?? name.ToNormalizedString() ?? name);
    
    public static Artist NewArtistFromName(string name) 
        => new Artist(name, name.ToNormalizedString() ?? name, name.ToTitleCase());
}
