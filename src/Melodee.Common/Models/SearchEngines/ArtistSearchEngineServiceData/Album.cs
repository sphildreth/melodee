using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;

[Index(nameof(ArtistId), nameof(NameNormalized), nameof(Year), IsUnique = true)]
public record Album
{
    [Key]
    public int Id { get; init; }
    
    public required Artist Artist { get; init; }
    
    [RequiredGreaterThanZero]
    public required int ArtistId { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public required string SortName { get; init; }
    
    [RequiredGreaterThanZero]
    public required int AlbumType { get; init; }

    public AlbumType AlbumTypeValue => SafeParser.ToEnum<AlbumType>(AlbumType);
    
    public Guid? MusicBrainzId { get; init; }
    
    public Guid? MusicBrainzReleaseGroupId { get; set; }

    [MaxLength(255)]
    public string? SpotifyId { get; init; }
    
    [MaxLength(2000)]
    public string? CoverUrl { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public required string Name { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public required string NameNormalized { get; init; }    
    
    [RequiredGreaterThanZero]
    public required int Year { get; init; }
}


