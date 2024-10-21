using System.ComponentModel.DataAnnotations;

namespace Melodee.Common.Data.Models;

public class PlayQueue : DataModelBase
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int SongId { get; set; }
    
    [Required]
    public decimal Position { get; set; }
}
