using ATL;

namespace Melodee.Plugins.MetaData.Song.Extensions;

public static class AtlSongExtensions
{
    public static FileInfo FileInfo(this Track song)
    {
        return new FileInfo(song.Path);
    }
}
