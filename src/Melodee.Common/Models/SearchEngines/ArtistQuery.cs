using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.SearchEngines;

public sealed record ArtistQuery
{
    public string NameNormalized => Name.ToNormalizedString() ?? Name;

    public Guid? MusicBrainzIdValue => SafeParser.ToGuid(MusicBrainzId);

    public string[]? AlbumNamesNormalized => AlbumKeyValues?.Where(x => x.Value != null).Select(x => x.Value.ToNormalizedString() ?? x.Value!).ToArray();

    public required string Name { get; init; }

    public KeyValue[]? AlbumKeyValues { get; init; }

    public string? MusicBrainzId { get; init; }
}
