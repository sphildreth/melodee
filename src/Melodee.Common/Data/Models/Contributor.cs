using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;

namespace Melodee.Common.Data.Models;

[Serializable]
public class Contributor : DataModelBase
{
    /// <summary>
    /// The contributor role.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Role { get; set; }

    /// <summary>
    /// The subRole for roles that may require it. Ex: The instrument for the performer role (TMCL/performer tags). Note: For consistency between different tag formats, the TIPL sub roles should be directly exposed in the role field.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SubRole { get; set; }

    /// <summary>
    /// This is the artist who did the contribution, not necessarily the song or album artist.
    /// </summary>
    [RequiredGreaterThanZero] public int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;

    /// <summary>
    ///     This is not set when it's an Album level contribution.
    /// </summary>
    public int? SongId { get; set; }

    public Song? Song { get; set; }

    /// <summary>
    ///     This is always set if Album or song contribution. 
    /// </summary>
    [RequiredGreaterThanZero]
    public required int AlbumId { get; set; }

    public Album Album { get; set; } = null!;
}
