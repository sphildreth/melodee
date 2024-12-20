using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Enums;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

/// <summary>
///     This is a materialized release for MusicBrainz release from all the MusicBrainz export files.
/// </summary>
public sealed record Album
{
    [AutoIncrement] public long Id { get; set; }

    public required long UniqueId { get; init; }

    public required long ArtistId { get; init; }

    [Index(false)] public required string Name { get; init; }

    public required string SortName { get; init; }

    [Index(false)] public required string NameNormalized { get; init; }

    public int ReleaseType { get; init; }
    
    [NotMapped] public ReleaseType ReleaseTypeValue => SafeParser.ToEnum<ReleaseType>(ReleaseType);

    public bool DoIncludeInArtistSearch => ReleaseDate > DateTime.MinValue &&
                                           ReleaseTypeValue != Enums.ReleaseType.Single &&
                                           ReleaseTypeValue != Enums.ReleaseType.Broadcast;

    [Index] public required string MusicBrainzIdRaw { get; init; }

    [NotMapped] public Guid MusicBrainzId => SafeParser.ToGuid(MusicBrainzIdRaw) ?? Guid.Empty;

    public required string ReleaseGroupMusicBrainzIdRaw { get; init; }
    
    [Ignore][NotMapped] public Guid ReleaseGroupMusicBrainzId => SafeParser.ToGuid(ReleaseGroupMusicBrainzIdRaw) ?? Guid.Empty;

    public required DateTime ReleaseDate { get; init; }

    public string? ContributorIds { get; init; }
}
