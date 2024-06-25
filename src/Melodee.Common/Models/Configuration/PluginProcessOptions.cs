namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record PluginProcessOptions
{
    public bool DoDeleteOriginal { get; set; } = true;
    
    public bool DoLoadEmbeddedImages { get; set; } = true;

    public int MaximumArtistDirectoryNameLength { get; init; } = 200;
    
    public int MaximumReleaseDirectoryNameLength { get; init; } = 200;

    public int MinimumValidReleaseYear { get; set; } = 1925;
}