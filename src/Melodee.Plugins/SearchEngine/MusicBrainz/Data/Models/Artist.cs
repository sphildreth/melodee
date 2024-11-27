using Melodee.Common.Extensions;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

/// <summary>
///     https://github.com/metabrainz/musicbrainz-server/blob/041164ea71530cc13b50305628a2727253de69a2/admin/sql/CreateTables.sql#L4
/// </summary>
public sealed record Artist
{
    [Ignore] public bool IsValid => MusicBrainzId != Guid.Empty && Name.Nullify() != null;

    public long Id { get; init; }

    [Index(Unique = true)] public required Guid MusicBrainzId { get; init; }

    [Index(Unique = false)] public required string Name { get; init; }

    [Index(Unique = false)] public required string NameNormalized { get; init; }

    public required string SortName { get; init; }
}
