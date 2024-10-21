using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserId), nameof(SongId), IsUnique = true)]
public class Bookmark : MetaDataModelBase
{
    [Required]
    public required int UserId { get; set; }
    
    [Required]
    public required int SongId { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? Comment { get; set; }
    
    public int Position { get; set; }
}
