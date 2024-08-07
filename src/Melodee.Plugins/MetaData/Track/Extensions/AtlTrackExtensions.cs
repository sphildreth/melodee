using Melodee.Common.Utility;
using Melodee.Plugins.Discovery.Releases;

namespace Melodee.Plugins.MetaData.Track.Extensions;

public static class AtlTrackExtensions
{
        public static FileInfo FileInfo(this ATL.Track track)
        {
            return new FileInfo(track.Path);
        }

        public static short DiskNumberValue(this ATL.Track track)
        {
            var r = SafeParser.ToNumber<short?>(track.DiscNumber) ?? 1;
            return r < ReleasesDiscoverer.MinimumDiscNumber ? ReleasesDiscoverer.MinimumDiscNumber : r;
        }

}