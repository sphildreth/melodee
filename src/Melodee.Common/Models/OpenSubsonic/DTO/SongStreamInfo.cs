namespace Melodee.Common.Models.OpenSubsonic.DTO;

public record SongStreamInfo(string Path, long FileSize, double Duration, string ContentType)
{
    public FileInfo TrackFileInfo => new(Path);
}
