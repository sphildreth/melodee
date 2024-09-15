namespace Melodee.Plugins.MetaData.Track.Extensions;

public static class AtlTrackExtensions
{
    public static FileInfo FileInfo(this ATL.Track track)
    {
        return new FileInfo(track.Path);
    }
}
