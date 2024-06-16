using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
/// This is a representation of a Release (a published collection of Tracks) including all known MetaData.
/// </summary>
public sealed record Release
{
    public long UniqueId => SafeParser.Hash($"{this.Artist()}{this.ReleaseYear()}{this.ReleaseTitle}"); 
    
    public required DirectoryInfo DirectoryInfo { get; init; }
    
    public required IEnumerable<FileInfo> FileInfos { get; init; }
    
    public IEnumerable<ImageInfo>? Images { get; init; }
    
    public IEnumerable<MetaTag<object>>? Tags { get; init; }
}