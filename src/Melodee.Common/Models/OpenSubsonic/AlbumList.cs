using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record AlbumList : AlbumList2
{
    public required decimal AverageRating { get; init; }

    public override string ToXml(string? nodeName = null)
    {
        return
            $"<album id=\"{Id}\" parent=\"{ArtistId}\" title=\"{Title.ToSafeXmlString()}\" artist=\"{Artist.ToSafeXmlString()}\" isDir=\"true\" coverArt=\"{CoverArt}\" userRating=\"{UserRating ?? 0}\" averageRating=\"{AverageRating}\"/>";
    }
}
