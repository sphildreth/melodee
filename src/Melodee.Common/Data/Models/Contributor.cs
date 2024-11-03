using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;

namespace Melodee.Common.Data.Models;

[Serializable]
public class Contributor : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Role { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SubRole { get; set; }
    
    [RequiredGreaterThanZero]
    public int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;
    
    /// <summary>
    /// This is not set when it's an Album level contribution.
    /// </summary>
    public int? SongId { get; set; }
    
    public Song? Song { get; set; }
    
    /// <summary>
    /// This is always set if Album or song contribution.
    /// </summary>
    [RequiredGreaterThanZero]
    public required int AlbumId { get; set; }
    
    public Album Album { get; set; } = null!;
}
