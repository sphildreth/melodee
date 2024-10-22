using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;

namespace Melodee.Common.Data.Models;

public class PlayQueue : DataModelBase
{
    [RequiredGreaterThanZero]
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [RequiredGreaterThanZero]
    public int SongId { get; set; }
    
    public Song Song { get; set; } = null!;
    
    [Required]
    public decimal Position { get; set; }
}
