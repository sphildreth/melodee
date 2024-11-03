using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(SongId), IsUnique = true)]
public class UserSong : DataModelBase
{
    [RequiredGreaterThanZero]
    public required int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [RequiredGreaterThanZero]
    public required int SongId { get; set; }
    
    public Song Song { get; set; } = null!;
    
    public int PlayedCount { get; set; }
    
    [Required]
    public required Instant LastPlayedAt { get; set; }
    
    public bool IsStarred { get; set; }
    
    public int Rating { get; set; }
}
