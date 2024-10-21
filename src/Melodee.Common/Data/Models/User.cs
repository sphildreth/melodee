using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string UserName { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    
    public Instant? LastLoginAt { get; set; }
    
    public Instant? LastActivityAt { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public bool HasSettingsRole { get; set; }
    
    public bool HasDownloadRole { get; set; }
    
    public bool HasUploadRole { get; set; }
    
    public bool HasPlaylistRole { get; set; }
    
    public bool HasCoverArtRole { get; set; }
    
    public bool HasCommentRole { get; set; }
    
    public bool HasPodcastRole { get; set; }
    
    public bool HasStreamRole { get; set; }
    
    public bool HasJukeboxRole { get; set; }
    
    public bool HasShareRole { get; set; }
    
    public bool IsScrobblingEnabled { get; set; }
}
