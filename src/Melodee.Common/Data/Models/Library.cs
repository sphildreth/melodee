using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

/// <summary>
/// Should be a single record for each type of library. A staging, an inbound, a library.
/// </summary>
[Index(nameof(Type), IsUnique = true)]
public class Library : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string Path { get; set; }
    
    [RequiredGreaterThanZero]
    public required int Type { get; set; }
    
    [NotMapped]
    public LibraryType TypeValue => SafeParser.ToEnum<LibraryType>(Type);
    
    public Instant? LastScanAt { get; set; }
    
    public ICollection<LibraryScanHistory> ScanHistories { get; set; } = new List<LibraryScanHistory>();

    public Instant LastWriteTime() => !Directory.Exists(Path) ? Instant.MinValue : Instant.FromDateTimeUtc(Directory.GetLastWriteTimeUtc(Path));

    public bool NeedsScanning()
    {
        if (!Directory.Exists(Path))
        {
            return false;
        }
        return LastScanAt == null || LastScanAt < LastWriteTime();
    }
}
