using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Enums;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

/// <summary>
///     https://github.com/metabrainz/musicbrainz-server/blob/041164ea71530cc13b50305628a2727253de69a2/admin/sql/CreateTables.sql#L4
/// </summary>
public sealed record Artist
{
    public bool IsValid => MusicBrainzId != Guid.Empty && Name.Nullify() != null;

    public long Id { get; init; }

    public required Guid MusicBrainzId { get; init; }

    public required string Name { get; init; }

    public required string NameNormalized { get; init; }

    public int ArtistType { get; init; }
    public ArtistType ArtistTypeValue => SafeParser.ToEnum<ArtistType>(ArtistType);

    public required string SortName { get; init; }
}
