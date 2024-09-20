using System.Text.Json.Serialization;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

[Serializable]
public sealed record Track
{
    public long ReleaseUniqueId => SafeParser.Hash(this.ReleaseArtist(), this.ReleaseYear().ToString(), this.ReleaseTitle());
    
    /// <summary>
    /// Unique TrackId on Release
    /// </summary>
    public long TrackId => SafeParser.Hash(this.MediaNumber().ToString(), this.TrackNumber().ToString(), this.Title());    
    
    /// <summary>
    /// Globally UnqiueId 
    /// </summary>
    public long UniqueId => SafeParser.Hash(ReleaseUniqueId.ToString(), this.TrackArtist(), this.TrackYear().ToString(), this.MediaNumber().ToString(), this.TrackNumber().ToString(), this.Title()); 
    
    public required string CrcHash { get; init; }
    
    public required FileSystemFileInfo File { get; init; }
    
    [JsonIgnore]
    public IEnumerable<ImageInfo>? Images { get; init; }
    
    public IEnumerable<MetaTag<object?>>? Tags { get; init; }
    
    public IEnumerable<MediaAudio<object?>>? MediaAudios { get; init; }

    public int SortOrder { get; set; }
    
    public override string ToString() => $"ReleaseId [{ReleaseUniqueId}] TrackId [{UniqueId}] File [{File}]";
}
