using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public class LibraryScanHistory 
{
    public int Id { get; set; }
    
    [Required]
    public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();    
    
    [RequiredGreaterThanZero]
    public required int LibraryId { get; set; }
    
    public Library Library { get; set; } = null!;
    
    public int? ForArtistId { get; set; }
    
    public int? ForAlbumId { get; set; }
    
    public int FoundArtistsCount { get; set; }
    
    public int FoundAlbumsCount { get; set; }
    
    public int FoundSongsCount { get; set; }
    
    [RequiredGreaterThanZero]
    public long DurationInMs { get; set; }

    [NotMapped] public Duration DurationInMillisecondsValue => Duration.FromMilliseconds(DurationInMs);
}
