using Melodee.Common.Extensions;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ReleaseGroupMeta
{
    public long ReleaseGroupId { get; init; }
    
    public DateTime ReleaseDate => DateTime.Parse($"{DateYearValue.ToStringPadLeft(4)}-{DateMonthValue.ToStringPadLeft(2)}-{DateDayValue.ToStringPadLeft(2)}T00:00:00");

    public int DateYearValue => DateYear > DateTime.MinValue.Year && DateYear < DateTime.MaxValue.Year ? DateYear : DateTime.MinValue.Year;
    
    public int DateMonthValue => DateMonth is > 0 and < 12 ? DateMonth : 1;
    
    public int DateDayValue => DateMonth is > 0 and < 31 ? DateMonth : 1;
    
    public bool IsValid => ReleaseGroupId > 0 && DateDayValue > 0 && DateMonthValue > 0 && DateYearValue > 0;
    
    public int DateYear { get; init; }

    public int DateMonth { get; init; }

    public int DateDay { get; init; }
}
