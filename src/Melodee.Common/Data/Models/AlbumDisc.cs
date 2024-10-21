using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Index(nameof(AlbumId), nameof(DiscNumber), IsUnique = true)]
public sealed class AlbumDisc
{
    [Required]
    public int AlbumId { get; set; }

    [ForeignKey(nameof(AlbumId))] 
    public Album Album { get; init; } = default!;
    
    public int DiscNumber { get; set; }
    
    public short? SongCount { get; set; }
    
    public string? Title { get; set; }
}
