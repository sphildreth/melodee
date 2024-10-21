using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class Share : DataModelBase
{
    [Required]
    public required int UserId { get; set; }

    /// <summary>
    /// Pipe seperated list.
    /// </summary>
    [Required]
    public required string SongIds { get; set; }

    [Required]
    public required Instant ExpiresAt { get; set; }
    
    public bool IsDownloadable { get; set; }
    
    public Instant? LastVisitedAt { get; set; }

    public int VisitCount { get; set; }
}
