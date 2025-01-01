using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ContributorName), nameof(MetaTagIdentifier), nameof(AlbumId), IsUnique = true)]
[Index(nameof(ArtistId), nameof(MetaTagIdentifier), nameof(AlbumId), IsUnique = true)]
public class Contributor : DataModelBase
{
    /// <summary>
    ///     The contributor role.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Role { get; set; }

    /// <summary>
    ///     The subRole for roles that may require it. Ex: The instrument for the performer role (TMCL/performer tags). Note:
    ///     For consistency between different tag formats, the TIPL sub roles should be directly exposed in the role field.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SubRole { get; set; }

    /// <summary>
    ///     This is the artist who did the contribution, not necessarily the song or album artist.
    /// </summary>
    public int? ArtistId { get; set; }

    public Artist? Artist { get; set; } = null!;

    /// <summary>
    ///     This is populated when no artist is found (or not an artist, like Publisher and Production)
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? ContributorName { get; set; }

    [RequiredGreaterThanZero] public int MetaTagIdentifier { get; set; }

    [NotMapped] public MetaTagIdentifier MetaTagIdentifierValue => SafeParser.ToEnum<MetaTagIdentifier>(MetaTagIdentifier);

    /// <summary>
    ///     This is not set when it's an Album level contribution.
    /// </summary>
    public int? SongId { get; set; }

    [NotMapped] public long SongUniqueId { get; set; }

    public Song? Song { get; set; }

    /// <summary>
    ///     This is always set if Album or song contribution.
    /// </summary>
    [RequiredGreaterThanZero]
    public required int AlbumId { get; set; }

    public Album Album { get; set; } = null!;

    public int ContributorType { get; set; }

    [NotMapped] public ContributorType ContributorTypeValue => SafeParser.ToEnum<ContributorType>(ContributorType);
}
