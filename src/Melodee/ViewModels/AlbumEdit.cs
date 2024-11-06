using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;

namespace Melodee.ViewModels;

public sealed class AlbumEdit
{
    [Required(ErrorMessage = "Artist name is required.")]
    [MaxLength(255, ErrorMessage = "Artist name cannot be longer than 255 characters.")]
    public required string Artist { get; set; }
    
    [Required(ErrorMessage = "Album title is required.")]
    [MaxLength(255, ErrorMessage = "Album title cannot be longer than 255 characters.")]
    public required string Title { get; set; }
    
    public string? Genre { get; set; }
    
    [RequiredGreaterThanZero(ErrorMessage = "Album release year is required.")]
    public required int Year { get; set; }
    
    public required SongEdit[] Songs { get; set; }

    public void SetSelected(SongEdit selectedSong)
    {
        foreach (var song in Songs)
        {
            song.IsSelected = song == selectedSong;
        }
    }
}
