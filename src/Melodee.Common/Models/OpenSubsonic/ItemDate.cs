namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
/// A date for a media item that may be just a year, or year-month, or full date.
/// </summary>
/// <param name="Year">The year</param>
/// <param name="Month">The month (1-12)</param>
/// <param name="Day">The day (1-31)</param>
public record ItemDate(
    int? Year,
    int? Month,
    int? Day
);
