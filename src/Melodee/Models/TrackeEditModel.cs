using Melodee.Common.Utility;

namespace Melodee.Models;

public class TrackEditModel
{
    public long TrackId => SafeParser.Hash(MediaNumber.ToString(), Number.ToString(), Title);
    
    public string? TrackArtist { get; set; }
    
    public int MediaNumber { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public int Number { get; set; }
    
}
