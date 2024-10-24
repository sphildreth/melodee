using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(AlbumId), IsUnique = true)]
public class UserAlbum : DataModelBase
{
    [RequiredGreaterThanZero]
    public required int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [RequiredGreaterThanZero]
    public required int AlbumId { get; set; }
    
    public Album Album { get; set; } = null!;
    
    public int PlayedCount { get; set; }
    
    [Required]
    public required Instant LastPlayedAt { get; set; }
    
    public bool IsStarred { get; set; }
    
    public int Rating { get; set; }
}
