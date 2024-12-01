using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models.Extensions;

public static class BookmarkExtensions
{
    public static string ToCoverArtId(this Bookmark bookmark) => bookmark.ToApiKey();
    
    public static string ToApiKey(this Bookmark bookmark) => $"bookmark{OpenSubsonicServer.ApiIdSeparator}{bookmark.ApiKey}";

    public static Common.Models.OpenSubsonic.Bookmark ToApiBookmark(this Bookmark bookmark)
    {
        return new Common.Models.OpenSubsonic.Bookmark(
            bookmark.Position,
            bookmark.User.UserName,
            bookmark.Comment,
            bookmark.CreatedAt.ToString(),
            bookmark.LastUpdatedAt?.ToString() ?? bookmark.CreatedAt.ToString(),
            bookmark.Song.ToApiChild(bookmark.Song.AlbumDisc.Album, bookmark.Song.UserSongs.FirstOrDefault())
        );
        
    }
}
