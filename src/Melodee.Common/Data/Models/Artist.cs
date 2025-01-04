using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(LibraryId))]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(NameNormalized))]
[Index(nameof(SortName))]
public sealed class Artist : MetaDataModelBase
{
    public const string ImageType = "Artist";
    public const string PrimaryImageFileName = "01-Band.jpg";

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string NameNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SortName { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? RealName { get; set; }

    /// <summary>
    ///     The directory that holds the files for the Artist, including Artist images and albums directories. This is inside a
    ///     library path directory.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    [Required]
    public required string Directory { get; set; }

    /// <summary>
    ///     Pipe seperated list. Example 'artist|albumartist|composer'
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Roles { get; set; }

    public int AlbumCount { get; set; }

    public int SongCount { get; set; }

    [RequiredGreaterThanZero] public required int LibraryId { get; set; }

    public Library Library { get; set; } = null!;

    /// <summary>
    ///     Stored in markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Biography { get; set; }

    /// <summary>
    ///     Be able to query for artists that need images.
    /// </summary>
    public int? ImageCount { get; set; }

    public int MetaDataStatus { get; set; } = SafeParser.ToNumber<int>(MetaDataModelStatus.ReadyToProcess);

    [NotMapped] public MetaDataModelStatus MetaDataStatusValue => SafeParser.ToEnum<MetaDataModelStatus>(MetaDataStatus);

    public ICollection<Album> Albums { get; set; } = new List<Album>();

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<UserArtist> UserArtists { get; set; } = new List<UserArtist>();

    [InverseProperty(nameof(ArtistRelation.Artist))]
    public ICollection<ArtistRelation> RelatedArtists { get; set; } = new List<ArtistRelation>();

    public override string ToString()
    {
        return $"Id [{Id}] Name [{Name}]";
    }
}
