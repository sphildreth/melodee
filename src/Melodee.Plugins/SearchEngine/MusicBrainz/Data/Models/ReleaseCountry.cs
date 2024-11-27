using NodaTime;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ReleaseCountry
{
    [Ignore] public LocalDate ReleaseDate => LocalDate.FromDateTime(new DateTime(DateYear, DateMonth, DateDay));
    
    [Ignore] public bool IsValid => ReleaseId > 0 && DateYear > 0 && DateMonth > 0 && DateDay > 0;
    
    public long ReleaseId { get; init; }
    
    public long CountryId { get; init; }
    
    public int DateYear { get; init; }
    
    public int DateMonth  { get; init; }
    
    public int DateDay { get; init; }
}
