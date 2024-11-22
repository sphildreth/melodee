using Melodee.Common.Models.OpenSubsonic;
using NodaTime;

namespace Melodee.Services.Extensions;

public static class LocalDateExtensions
{
    public static ItemDate ToItemDate(this LocalDate localDate)
    {
        return new ItemDate(localDate.Year, localDate.Month, localDate.Day);
    }
}
