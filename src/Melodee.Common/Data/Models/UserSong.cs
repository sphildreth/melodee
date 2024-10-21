using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(SongId), IsUnique = true)]
public class UserSong : DataModelBase
{
    [Required]
    public required int UserId { get; set; }
    
    [Required]
    public required int SongId { get; set; }
    
    public int PlayedCount { get; set; }
    
    [Required]
    public required Instant LastPlayedAt { get; set; }
    
    public bool IsStarred { get; set; }
    
    public int Rating { get; set; }
}
