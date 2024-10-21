using System.ComponentModel.DataAnnotations;
using Melodee.Common.Models.OpenSubsonic;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(AlbumId), IsUnique = true)]
public class UserAlbum : DataModelBase
{
    [Required]
    public required int UserId { get; set; }
    
    [Required]
    public required int AlbumId { get; set; }
    
    public int PlayedCount { get; set; }
    
    [Required]
    public required Instant LastPlayedAt { get; set; }
    
    public bool IsStarred { get; set; }
    
    public int Rating { get; set; }
}
