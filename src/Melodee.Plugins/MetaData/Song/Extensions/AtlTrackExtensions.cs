namespace Melodee.Plugins.MetaData.Song.Extensions;

public static class AtlSongExtensions
{
    public static FileInfo FileInfo(this ATL.Track song)
    {
        return new FileInfo(song.Path);
    }
}
