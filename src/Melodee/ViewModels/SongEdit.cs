using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;

namespace Melodee.ViewModels;

public sealed class SongEdit
{
    public bool IsSelected { get; set; }
    
    [RequiredGreaterThanZero(ErrorMessage = "Song disc number (TSST) is required and must be greater than zero.")]
    public int DiscNumber { get; set; }
    
    [RequiredGreaterThanZero(ErrorMessage = "Song Number is required and must be greater than zero.")]
    public int SongNumber { get; set; }
    
    [Required(ErrorMessage = "Song title (TIT2) is required.")]
    public string? Title { get; set; }
    
    [MaxLength(255, ErrorMessage = "Song Subtitle must be under 255 characters.")]
    public string? PartTitles { get; set; }
    
    [MaxLength(255, ErrorMessage = "Song Artist must be under 255 characters.")]
    public string? SongArtist { get; set; }
    
    public string? Duration { get; init; }
}
