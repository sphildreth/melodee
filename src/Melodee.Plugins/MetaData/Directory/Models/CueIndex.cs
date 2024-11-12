namespace Melodee.Plugins.MetaData.Directory.Models;

public sealed record CueIndex
{
    public required int SongNumber { get; init; }

    public required int IndexNumber { get; init; }

    public required int Minutes { get; init; }

    public required int Seconds { get; init; }

    public required int Frames { get; init; }
}
