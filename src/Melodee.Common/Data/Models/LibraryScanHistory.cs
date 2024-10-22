using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class LibraryScanHistory : DataModelBase
{
    [RequiredGreaterThanZero]
    public required int LibraryId { get; set; }
    
    public Library Library { get; set; } = null!;
    
    public int? ForArtistId { get; set; }
    
    public int? ForAlbumId { get; set; }
    
    public int NewArtistsCount { get; set; }
    
    public int NewAlbumsCount { get; set; }
    
    public int NewSongsCount { get; set; }
    
    [Required]
    public required Duration ScanTimeScan { get; set; }
}
