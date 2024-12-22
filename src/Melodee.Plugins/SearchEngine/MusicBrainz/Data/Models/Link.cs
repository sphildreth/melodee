using System.Globalization;
using Melodee.Common.Extensions;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record Link
{
    public long Id { get; init; }

    public long LinkTypeId { get; init; }

    public DateTime? BeginDate => BeginDateYear == null ? null : MusicBrainzRepositoryBase.ParseJackedUpMusicBrainzDate($"{BeginDateYear.ToStringPadLeft(4)}-{BeginDateMonthValue.ToStringPadLeft(2)}-{BeginDateDayValue.ToStringPadLeft(2)}T00:00:00");

    public DateTime? EndDate => EndDateYear == null ? null : MusicBrainzRepositoryBase.ParseJackedUpMusicBrainzDate($"{EndDateYear.ToStringPadLeft(4)}-{EndDateMonthValue.ToStringPadLeft(2)}-{EndDateDayValue.ToStringPadLeft(2)}T00:00:00");

    public int? BeginDateYear { get; init; }

    public int? BeginDateMonth { get; init; }
    
    public int BeginDateMonthValue => BeginDateMonth ?? 1;

    public int BeginDateDayValue => BeginDateDay ?? 1;
    
    public int? BeginDateDay { get; init; }

    public int? EndDateYear { get; init; }

    public int? EndDateMonth { get; init; }
    
    public int EndDateMonthValue => EndDateMonth ?? 1;

    public int? EndDateDay { get; init; }
    
    public int EndDateDayValue => EndDateDay ?? 1;

    public bool IsEnded { get; init; }
}
