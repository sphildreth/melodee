using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;

namespace Melodee.ViewModels;

public sealed class DiscEdit
{
    public bool IsSelected { get; set; }
    
    [RequiredGreaterThanZero(ErrorMessage = "Disc number (TPOS) is required and must be greater than zero.")]
    public int DiscNumber { get; set; }
    
    [MaxLength(255, ErrorMessage = "Disc Subtitle must be under 255 characters.")]
    public string? PartTitles { get; set; }
    
    public int SongCount { get; init; }
}
