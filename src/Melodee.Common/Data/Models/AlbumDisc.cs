using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Index(nameof(AlbumId), nameof(DiscNumber), IsUnique = true)]
[PrimaryKey(nameof(AlbumId), nameof(DiscNumber))]
public sealed class AlbumDisc
{
    [RequiredGreaterThanZero]
    public int AlbumId { get; set; }

    public Album Album { get; init; } = null!;
    
    [RequiredGreaterThanZero]
    public int DiscNumber { get; set; }
    
    public short? SongCount { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Title { get; set; }
    
    public ICollection<Song> Songs { get; set; } = new List<Song>();
}
