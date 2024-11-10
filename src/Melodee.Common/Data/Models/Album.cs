using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public sealed class Album : NamedMetaDataModelBase
{
    [RequiredGreaterThanZero] public int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;

    [RequiredGreaterThanZero] public long MediaUniqueId { get; set; }

    public short AlbumStatus { get; set; }

    [NotMapped] public AlbumStatus AlbumStatusValue => SafeParser.ToEnum<AlbumStatus>(AlbumStatus);

    public int MetaDataStatus { get; set; } = SafeParser.ToNumber<int>(MetaDataModelStatus.ReadyToProcess);

    [NotMapped] public MetaDataModelStatus MetaDataStatusValue => SafeParser.ToEnum<MetaDataModelStatus>(MetaDataStatus);

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
    ///     Pipe seperated list. These are strictly defined in the Genre enum.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Genres { get; set; }
    
    /// <summary>
    /// The directory that holds the files for the Album. This is inside of a library path directory.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string Directory { get; set; }

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<AlbumDisc> Discs { get; set; } = new List<AlbumDisc>();

    public ICollection<UserAlbum> UserAlbums { get; set; } = new List<UserAlbum>();
}
