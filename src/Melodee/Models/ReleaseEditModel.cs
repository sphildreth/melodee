namespace Melodee.Models;

public class ReleaseEditModel
{
    public long UniqueId { get; init; }
    
    public string? Title { get; set; }
    
    public string? Year { get; set; }
    
    public string? Artist { get; set; }
}
