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
    
    public bool DoRenameConverted { get; set; } = true;

    public string ConvertedExtension { get; init; } = "_converted";
    
    public string ProcessedExtension { get; init; } = "_processed";
    
    public string SkippedExtension { get; init; } = "_skipped";
    
    public bool DoOverrideExistingMelodeeDataFiles { get; set; }

    public bool DoLoadEmbeddedImages { get; init; } = true;
    
    /// <summary>
    /// If set then limit the processing, otherwise process all found.
    /// </summary>
    public int? MaximumProcessingCount { get; init; }

    public int MaximumProcessingCountValue => MaximumProcessingCount ?? int.MaxValue; 
    
    public int MaximumArtistDirectoryNameLength { get; init; } = 200;

    public int MaximumReleaseDirectoryNameLength { get; init; } = 200;

    public int MinimumValidReleaseYear { get; init; } = 1925;

    public IEnumerable<string> ReleaseTitleRemovals { get; init; } = new[]
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

    public bool DoContinueOnDirectoryProcessingErrors { get; init; } = true;
    
    /// <summary>
    /// When true then move Release Melodee json files to the Staging directory.
    /// </summary>
    public bool DoMoveToStagingDirectory { get; init; }= true;
}
