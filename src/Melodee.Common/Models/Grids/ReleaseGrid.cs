using Melodee.Common.Enums;

namespace Melodee.Common.Models.Grids;

[Serializable]
public sealed record ReleaseGrid
{
    public bool IsChecked { get; set; }
    
    /// <summary>
    /// What plugins were utilized in discovering this release.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; init; }
    
    public string? Artist { get; init; }
    
    public string? Title { get; init; }
    
    public int? Year { get; init; }
    
    public int TrackCount { get; init; }
    
    public required ReleaseStatus ReleaseStatus { get; init; }
}