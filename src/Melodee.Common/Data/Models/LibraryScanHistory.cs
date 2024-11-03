using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
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
    
    [RequiredGreaterThanZero]
    public long DurationInMs { get; set; }

    [NotMapped] public Duration DurationInMillisecondsValue => Duration.FromMilliseconds(DurationInMs);
}
