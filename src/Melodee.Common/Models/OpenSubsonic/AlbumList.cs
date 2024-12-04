using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record AlbumList : AlbumList2
{
    public required string ParentId { get; init; }
    
    public required bool IsDir { get; init; }
    
    public required decimal AverageRating { get; init; }
    
    public override string ToXml(string? nodeName = null)
    {
        return $"<album id=\"{ Id }\" parent=\"{ ParentId }\" title=\"{ Title }\" artist=\"{ Artist }\" isDir=\"{ IsDir.ToLowerCaseString() }\" coverArt=\"{ CoverArt }\" userRating=\"{ UserRating }\" averageRating=\"{ AverageRating }\"/>";
    }    
}
