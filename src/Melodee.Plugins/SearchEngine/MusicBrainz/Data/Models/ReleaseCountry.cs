using Melodee.Common.Extensions;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ReleaseCountry
{
    public long Id { get; init; }

    public DateTime? ReleaseDate => IsValid ? null : DateTime.Parse($"{DateYearValue.ToStringPadLeft(4)}-{DateMonthValue.ToStringPadLeft(2)}-{DateDayValue.ToStringPadLeft(2)}T00:00:00");

    public int DateYearValue => DateYear > DateTime.MinValue.Year && DateYear < DateTime.MaxValue.Year ? DateYear : DateTime.MinValue.Year;
    
    public int DateMonthValue => DateMonth is > 0 and < 12 ? DateMonth : 1;
    
    public int DateDayValue => DateMonth is > 0 and < 31 ? DateMonth : 1;
    
    public bool IsValid => ReleaseId > 0 && ReleaseDate != null;

    public long ReleaseId { get; init; }

    public long CountryId { get; init; }

    public int DateYear { get; init; }

    public int DateMonth { get; init; }

    public int DateDay { get; init; }
}
