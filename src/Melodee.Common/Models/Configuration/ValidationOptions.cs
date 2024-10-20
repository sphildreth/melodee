namespace Melodee.Common.Models.Configuration;

public sealed record ValidationOptions
{
    public short MinimumMediaNumber { get; set; } = 1;

    public int MaximumMediaNumber { get; set; } = 500;

    public int MaximumSongNumber { get; set; } = 1000;

    public int MinimumAlbumYear { get; set; } = 1860;

    public int MaximumAlbumYear { get; set; } = 2200;
}
