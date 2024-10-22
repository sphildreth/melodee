using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(ServiceUrl), nameof(SongId), nameof(PlayTime), IsUnique = true)]
public class Scrobble : DataModelBase
{
    [RequiredGreaterThanZero]
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]   
    public required string ServiceUrl { get; set; }
    
    [RequiredGreaterThanZero]
    public int SongId { get; set; }
    
    public Song Song { get; set; } = null!;
    
    [Required]
    public Duration PlayTime { get; set; }
    
    [Required]
    public Instant EnqueueTime { get; set; }
}
