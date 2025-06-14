using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Enums;
using Melodee.Common.Utility;
using ServiceStack.DataAnnotations;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

/// <summary>
///     This is a materialized release for MusicBrainz release from all the MusicBrainz export files.
/// </summary>
public sealed record Album
{
    public const string TableName = "albums";

    [AutoIncrement] public long Id { get; set; }

    /// <summary>
    ///     This is the MusicBrainz database Id
    /// </summary>
    [Index(false)]
    public required long MusicBrainzArtistId { get; init; }

    [Index(false)]
    [StringLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string Name { get; init; }

    [StringLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string SortName { get; init; }

    [Index(false)]
    [StringLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string NameNormalized { get; init; }

    public int ReleaseType { get; init; }

    [Ignore] [NotMapped] public ReleaseType ReleaseTypeValue => SafeParser.ToEnum<ReleaseType>(ReleaseType);

    public bool DoIncludeInArtistSearch => ReleaseDate > DateTime.MinValue;

    [Index]
    [StringLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string MusicBrainzIdRaw { get; init; }

    [Ignore] [NotMapped] public Guid MusicBrainzId => SafeParser.ToGuid(MusicBrainzIdRaw) ?? Guid.Empty;

    [StringLength(MusicBrainzRepositoryBase.MaxIndexSize)]
    public required string ReleaseGroupMusicBrainzIdRaw { get; init; }

    [Ignore] [NotMapped] public Guid ReleaseGroupMusicBrainzId => SafeParser.ToGuid(ReleaseGroupMusicBrainzIdRaw) ?? Guid.Empty;

    public required DateTime ReleaseDate { get; init; }

    public string? ContributorIds { get; init; }
}
