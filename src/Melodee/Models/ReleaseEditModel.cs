using System.Runtime.Serialization.Json;

namespace Melodee.Models;

public class ReleaseEditModel
{
    public long UniqueId { get; init; }
    
    public string? Title { get; set; }
    
    public string? Year { get; set; }
    
    public string? Artist { get; set; }

    public int[] MediaNumbers => Tracks.GroupBy(x => x.MediaNumber).Select(x => x.Key).ToArray().Distinct().OrderBy(x => x).ToArray();
    
    public List<TrackEditModel> Tracks { get; set; } = [];
    
    public string? ReleaseStatus { get; set; }
}
