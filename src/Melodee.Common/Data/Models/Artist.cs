using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models;

[Serializable]
public class Artist : MetaDataModelBase
{
    [RequiredGreaterThanZero] public long MediaUniqueId { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? RealName { get; set; }

    /// <summary>
    ///     Pipe seperated list. Example 'artist|albumartist|composer'
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Roles { get; set; }

    public int AlbumCount { get; set; }

    public int SongCount { get; set; }

    /// <summary>
    ///     Stored in markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Biography { get; set; }
    
    public int MetaDataStatus { get; set; } = SafeParser.ToNumber<int>(MetaDataModelStatus.ReadyToProcess);
   
    [NotMapped] public MetaDataModelStatus MetaDataStatusValue => SafeParser.ToEnum<MetaDataModelStatus>(MetaDataStatus);     

    public ICollection<Album> Albums { get; set; } = new List<Album>();

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<UserArtist> UserArtists { get; set; } = new List<UserArtist>();
}
