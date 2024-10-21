using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(ServiceUrl), nameof(SongId), nameof(PlayTime), IsUnique = true)]
public class Scrobble : DataModelBase
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public required string ServiceUrl { get; set; }
    
    [Required]
    public int SongId { get; set; }
    
    [Required]
    public Duration PlayTime { get; set; }
    
    [Required]
    public Instant EnqueueTime { get; set; }
}
