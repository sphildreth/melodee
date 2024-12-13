using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

/// <summary>
///     This is a materialized record for MusicBrainz Artist from all the MusicBrainz export files.
/// </summary>
public sealed record Artist
{
    private string[]? _alternateNames;

    [AutoIncrement] public long Id { get; set; }


    public required long UniqueId { get; init; }

    public required long MusicBrainzArtistId { get; init; }

    [Index(false)] public required string Name { get; init; }

    public required string SortName { get; init; }

    [Index(false)] public required string NameNormalized { get; init; }

    [Index] public required string MusicBrainzIdRaw { get; init; }

    [NotMapped] public Guid MusicBrainzId => SafeParser.ToGuid(MusicBrainzIdRaw) ?? Guid.Empty;

    [Index(false)] public string? AlternateNames { get; init; }

    [NotMapped]
    public string[] AlternateNamesValues => _alternateNames ??= AlternateNames?.ToTags()?.ToArray() ?? [];
}
