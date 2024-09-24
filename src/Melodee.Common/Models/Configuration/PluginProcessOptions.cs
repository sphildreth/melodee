namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record PluginProcessOptions
{
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

    public bool DoUseCurrentYearAsDefaultOrigReleaseYearValue = true;
    public bool DoDeleteOriginal { get; set; } = true;

    public bool DoOverrideExistingMelodeeDataFiles { get; set; }

    public bool DoLoadEmbeddedImages { get; set; } = true;
    
    /// <summary>
    /// If set then limit the processing, otherwise process all found.
    /// </summary>
    public int? MaximumProcessingCount { get; set; }

    public int MaximumProcessingCountValue => MaximumProcessingCount ?? int.MaxValue; 
    
    public int MaximumArtistDirectoryNameLength { get; set; } = 200;

    public int MaximumReleaseDirectoryNameLength { get; set; } = 200;

    public int MinimumValidReleaseYear { get; set; } = 1925;

    public IEnumerable<string> ReleaseTitleRemovals { get; set; } = new[]
    {
        "^",
        "~",
        "#"
    };

    public IEnumerable<string> TrackTitleRemovals { get; set; } = new[]
    {
        ";",
        "(Remaster)",
        "Remaster"
    };

    public bool DoContinueOnDirectoryProcessingErrors { get; set; } = true;
    
    /// <summary>
    /// When true then move Release Melodee json files to the Staging directory.
    /// </summary>
    public bool DoMoveToStagingDirectory { get; set; }= true;
}
