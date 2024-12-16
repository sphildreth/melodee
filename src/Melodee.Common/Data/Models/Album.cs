using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ArtistId), nameof(Name), IsUnique = true)]
[Index(nameof(ArtistId),nameof(NameNormalized), IsUnique = true)]
[Index(nameof(ArtistId), nameof(SortName), IsUnique = true)]
public sealed class Album : MetaDataModelBase
{
    public const string FrontImageType = "Front";    
    
    [RequiredGreaterThanZero] public int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string NameNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SortName { get; set; }    

    [RequiredGreaterThanZero] public long MediaUniqueId { get; set; }

    public short AlbumStatus { get; set; }

    [NotMapped] public AlbumStatus AlbumStatusValue => SafeParser.ToEnum<AlbumStatus>(AlbumStatus);

    public int MetaDataStatus { get; set; } = SafeParser.ToNumber<int>(MetaDataModelStatus.ReadyToProcess);

    [NotMapped] public MetaDataModelStatus MetaDataStatusValue => SafeParser.ToEnum<MetaDataModelStatus>(MetaDataStatus);

    /// <summary>
    /// Be able to query for albums that need images.
    /// </summary>
    public int? ImageCount { get; set; }    
    
    public short AlbumType { get; set; }

    [NotMapped] public AlbumType AlbumTypeValue => SafeParser.ToEnum<AlbumType>(AlbumType);

    /// <summary>
    ///     Date the album was originally released. If not a re-release this value is null.
    /// </summary>
    public LocalDate? OriginalReleaseDate { get; set; }

    /// <summary>
    ///     Date the specific edition of the album was released.
    /// </summary>
    public LocalDate ReleaseDate { get; set; }

    public bool IsCompilation { get; set; }

    public short? SongCount { get; set; }

    public short? DiscCount { get; set; }

    public double Duration { get; set; }

    /// <summary>
    /// These are strictly defined in the Genre enum. 
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string[]? Genres { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string[]? Moods { get; set; }
    
    /// <summary>
    /// This is plain text and served to OpenSubsonic clients.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Comment { get; set; }    
    
    /// <summary>
    /// The album replay gain value. (In Db)
    /// </summary>
    public double? ReplayGain { get; set; }
    
    /// <summary>
    /// The album peak value. (Must be positive)
    /// </summary>
    public double? ReplayPeak { get; set; }    

    /// <summary>
    ///     The directory that holds the files for the Album. This is inside of an artist path and that is inside a library path directory.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string Directory { get; set; }

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<AlbumDisc> Discs { get; set; } = new List<AlbumDisc>();

    public ICollection<UserAlbum> UserAlbums { get; set; } = new List<UserAlbum>();

    public override string ToString() => $"Id [{Id}] ApiKey [{ApiKey}] ArtistId [{ArtistId}] Name [{Name}] Directory [{Directory}]";
}
