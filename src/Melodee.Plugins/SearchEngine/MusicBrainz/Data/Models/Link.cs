using Melodee.Common.Extensions;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record Link
{
    public long Id { get; init; }

    public long LinkTypeId { get; init; }

    public DateTime? BeginDate => BeginDateYear == null ? null : DateTime.Parse($"{BeginDateYear.ToStringPadLeft(4)}-{BeginDateMonth.ToStringPadLeft(2)}-{BeginDateDay.ToStringPadLeft(2)}T00:00:00");

    public DateTime? EndDate => EndDateYear == null ? null : DateTime.Parse($"{EndDateYear.ToStringPadLeft(4)}-{EndDateMonth.ToStringPadLeft(2)}-{EndDateDay.ToStringPadLeft(2)}T00:00:00");

    public int? BeginDateYear { get; init; }

    public int? BeginDateMonth { get; init; }

    public int? BeginDateDay { get; init; }

    public int? EndDateYear { get; init; }

    public int? EndDateMonth { get; init; }

    public int? EndDateDay { get; init; }

    public bool IsEnded { get; init; }
}
