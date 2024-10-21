using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Index(nameof(SongId), nameof(PlaylistId), IsUnique = true)]
public class PlaylistSong 
{
    [Required]
    public int SongId { get; set; }
    
    [Required]
    public int PlaylistId { get; set; }
    
    public int PlaylistOrder { get; set; }
}
