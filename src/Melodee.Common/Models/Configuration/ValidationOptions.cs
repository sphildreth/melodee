namespace Melodee.Common.Models.Configuration;

public sealed record ValidationOptions
{
    public short MinimumMediaNumber { get; init; } = 1;
   
    public int MaximumMediaNumber { get; init; } = 500;
   
    public int MaximumTrackNumber { get; init; } = 1000;

    public int MinimumReleaseYear { get; init; } = 1860;
    
    public int MaximumReleaseYear { get; init; } = 2200;
}
