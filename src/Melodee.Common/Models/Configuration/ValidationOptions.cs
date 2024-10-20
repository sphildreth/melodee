namespace Melodee.Common.Models.Configuration;

public sealed record ValidationOptions
{
    public short MinimumMediaNumber { get; set; } = 1;

    public int MaximumMediaNumber { get; set; } = 500;

    public int MaximumTrackNumber { get; set; } = 1000;

    public int MinimumReleaseYear { get; set; } = 1860;

    public int MaximumReleaseYear { get; set; } = 2200;
}
