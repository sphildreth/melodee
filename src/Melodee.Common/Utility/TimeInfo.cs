namespace Melodee.Common.Utility;

public sealed class TimeInfo
{
    public TimeInfo(decimal milliseconds)
    {
        var secondsTotal = milliseconds / 1000;
        var minutesTotal = Math.Floor(secondsTotal / 60);
        var hoursTotal = Math.Floor(minutesTotal / 60);
        var daysTotal = Math.Floor(hoursTotal / 24);
        var yearsTotal = Math.Floor(daysTotal / 365);

        Seconds = Math.Floor(secondsTotal - minutesTotal * 60);
        Minutes = minutesTotal - hoursTotal * 60;
        Hours = hoursTotal - daysTotal * 24;
        Days = daysTotal - yearsTotal * 365;
        Years = Math.Floor(daysTotal / 365);
    }

    public decimal Days { get; set; }

    public string DaysFormatted
    {
        get
        {
            if (Days > -1)
            {
                return Days.ToString("000");
            }

            return "--";
        }
    }

    public decimal Hours { get; set; }

    public string HoursFormatted
    {
        get
        {
            if (Hours > -1)
            {
                return Hours.ToString("00");
            }

            return "--";
        }
    }
    
    public int TotalMinutes => (int) (Hours * 60 + Minutes);

    public decimal Minutes { get; set; }

    public string MinutesFormatted
    {
        get
        {
            if (Minutes > -1)
            {
                return Minutes.ToString("00");
            }

            return "--";
        }
    }

    public decimal Seconds { get; set; }

    public string SecondsFormatted
    {
        get
        {
            if (Seconds > -1)
            {
                return Seconds.ToString("00");
            }

            return "--";
        }
    }

    public decimal? Years { get; set; }

    public string? YearsFormatted
    {
        get
        {
            if (Years.HasValue && Years.Value > 0)
            {
                return Years.Value.ToString("0");
            }

            return null;
        }
    }

    public string ToFullFormattedString()
    {
        var yearsFormatted = YearsFormatted;
        return $"{(string.IsNullOrEmpty(yearsFormatted) ? string.Empty : yearsFormatted + ":")}{ToString()}";
    }

    public string ToShortFormattedString()
    {
        return $"{MinutesFormatted}:{SecondsFormatted}";
    }

    public override string ToString()
    {
        if (Days > 0)
        {
            return $"{DaysFormatted}:{HoursFormatted}:{MinutesFormatted}:{SecondsFormatted}";
        }

        return $"{HoursFormatted}:{MinutesFormatted}:{SecondsFormatted}";
    }
}
