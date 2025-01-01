using ATL;

namespace Melodee.Common.Plugins.MetaData.Song.Extensions;

public static class AtlSongExtensions
{
    public static FileInfo FileInfo(this Track song)
    {
        return new FileInfo(song.Path);
    }
}
