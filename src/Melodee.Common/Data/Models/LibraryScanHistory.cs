using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class LibraryScanHistory : DataModelBase
{
    [Required]
    public required int LibraryId { get; set; }
    
    public int? ForArtistId { get; set; }
    
    public int? ForAlbumId { get; set; }
    
    public int NewArtistsCount { get; set; }
    
    public int NewAlbumsCount { get; set; }
    
    public int NewSongsCount { get; set; }
    
    [Required]
    public required Duration ScanTimeScan { get; set; }
}
