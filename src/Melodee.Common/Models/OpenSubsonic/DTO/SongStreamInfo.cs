using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic.DTO;

public record SongStreamInfo(string Path, long FileSize, double Duration, int BitRate, string ContentType)
{
    public FileInfo TrackFileInfo => new(Path);
}
