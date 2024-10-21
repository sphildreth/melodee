using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models;

public class Artist : MetaDataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? RealName { get; set; }
    
    /// <summary>
    /// Pipe seperated list. Example 'artist|albumartist|composer'
    /// </summary>
    public string? Roles { get; set; }    
    
    public int AlbumCount { get; set; }
    
    public int SongCount { get; set; }
    
    /// <summary>
    /// Should be markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>    
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]    
    public string? Biography { get; set; }
}
