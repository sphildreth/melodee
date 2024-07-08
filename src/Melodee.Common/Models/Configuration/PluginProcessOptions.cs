namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record PluginProcessOptions
{
    public bool DoDeleteOriginal { get; set; } = true;
    
    public bool DoLoadEmbeddedImages { get; set; } = true;

    public int MaximumArtistDirectoryNameLength { get; init; } = 200;
    
    public int MaximumReleaseDirectoryNameLength { get; init; } = 200;

    public int MinimumValidReleaseYear { get; init; } = 1925;

    public bool DoUseCurrentYearAsDefaultOrigReleaseYearValue = true;

    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ArtistNameReplacements = new[]
    {
        new KeyValuePair<string, IEnumerable<string>>("AC/DC", new[]
        {
            "AC; DC", "AC;DC", "AC/ DC", "AC DC"
        }),
        new KeyValuePair<string, IEnumerable<string>>("Love/Hate", new[]
        {
            "Love; Hate", "Love;Hate", "Love/ Hate", "Love Hate"
        })        
    };
    
    public IEnumerable<string> ReleaseTitleRemovals { get; init; }  = new[]
    {
        "^",
        "~",
        "#"
    };
    
    public IEnumerable<string> TrackTitleRemovals { get; init; } = new[]
    {
        ";",
        "(Remaster)",
        "Remaster"
    };
}