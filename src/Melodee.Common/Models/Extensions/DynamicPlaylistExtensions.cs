using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Extensions;

public static class DynamicPlaylistExtensions
{
    public static string? PrepareSongSelectionWhere(this DynamicPlaylist dynamicPlaylist, UserInfo user)
    {
        if (dynamicPlaylist.SongSelectionWhere.Nullify() == null)
        {
            return null;
        }

        return dynamicPlaylist.SongSelectionWhere.Replace("%userId%", user.Id.ToString());
    }
}
