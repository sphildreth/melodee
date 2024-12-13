using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

/// <summary>
///     This is a materialized record for MusicBrainz Artist to Artist relation from all the MusicBrainz export files.
/// </summary>
public sealed record ArtistRelation
{
    [AutoIncrement] public long Id { get; init; }

    [Index] public required long ArtistId { get; init; }

    [Index] public required long RelatedArtistId { get; init; }

    public required int ArtistRelationType { get; set; }

    [NotMapped] public ArtistRelationType ArtistRelationTypeValue => SafeParser.ToEnum<ArtistRelationType>(ArtistRelationType);

    public int SortOrder { get; init; }

    public required DateTime? RelationStart { get; init; }

    public required DateTime? RelationEnd { get; init; }
}
