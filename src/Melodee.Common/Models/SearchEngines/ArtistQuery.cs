using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.SearchEngines;

public sealed record ArtistQuery
{
    public KeyValue KeyValue => new KeyValue(SafeParser.Hash(MusicBrainzIdValue?.ToString() ?? NameNormalized).ToString(), NameNormalized);
    
    public string NameNormalized => Name.ToNormalizedString() ?? Name;

    public Guid? MusicBrainzIdValue => SafeParser.ToGuid(MusicBrainzId);

    public string[]? AlbumNamesNormalized => AlbumKeyValues?.Where(x => x.Value != null).Select(x => x.Value!).ToArray();

    public required string Name { get; init; }

    /// <summary>
    /// Album information of Year and NormalizedName
    /// </summary>
    public KeyValue[]? AlbumKeyValues { get; init; }

    public string? MusicBrainzId { get; init; }
}
