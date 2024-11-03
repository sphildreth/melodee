using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(SongId), nameof(PlaylistId), IsUnique = true)]
[PrimaryKey(nameof(SongId), nameof(PlaylistId))]
public class PlaylistSong 
{
    [RequiredGreaterThanZero]
    public int SongId { get; set; }
    
    public Song Song { get; set; } = null!;
    
    [RequiredGreaterThanZero]
    public int PlaylistId { get; set; }
    
    public Playlist Playlist { get; set; } = null!;
    
    public int PlaylistOrder { get; set; }
}
