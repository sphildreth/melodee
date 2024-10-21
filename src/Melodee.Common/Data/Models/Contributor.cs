using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models;


public class Contributor : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Role { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SubRole { get; set; }
    
    public int ArtistId { get; set; }
    
    /// <summary>
    /// This is not set when its a Album level contribution.
    /// </summary>
    public int? TrackId { get; set; }
    
    /// <summary>
    /// This is always set if Album or song contribution.
    /// </summary>
    [Required]
    public required int AlbumId { get; set; }
}
