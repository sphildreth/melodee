using System.ComponentModel.DataAnnotations;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.SearchEngines;

public record Query
{
    public Guid? ApiKey { get; init; }
    
    [Required]
    public required string Name { get; init; }

    public string NameReversed => string.Join(string.Empty, Name.Split(' ').Reverse()).ToNormalizedString() ?? Name;
    
    public string NameNormalized => Name.ToNormalizedString() ?? Name;
    
    public string QueryNameNormalizedValue => NameNormalized.Nullify() == null ? string.Empty : $"%{NameNormalized}%";

    public Guid? MusicBrainzIdValue => SafeParser.ToGuid(MusicBrainzId);
    
    public string? MusicBrainzId { get; init; }   
}
