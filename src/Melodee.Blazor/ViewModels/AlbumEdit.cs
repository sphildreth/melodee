using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Blazor.ViewModels;

public sealed class AlbumEdit
{
    [Required(ErrorMessage = "Artist is required.")]
    public required ArtistSearchResult ArtistSearchResult { get; set; }

    [Required(ErrorMessage = "Album title is required.")]
    [MaxLength(255, ErrorMessage = "Album title cannot be longer than 255 characters.")]
    public required string Title { get; set; }

    public string? Genre { get; set; }
    
    public string? MusicBrainzId { get; set; }

    [RequiredGreaterThanZero(ErrorMessage = "Album release year is required.")]
    public required int Year { get; set; }

    public required SongEdit[] Songs { get; set; }

    public required FileEdit[] AlbumDirectoryFiles { get; set; }
    
    public string? AmgId { get; set; }
    public string? SortName { get; set; }
    
    public int SortOrder { get; set; }
    
    public string? SpotifyId { get; set; }

    public List<string> AlternateNames = [];

    public List<string> Tags = [];    
    
}
