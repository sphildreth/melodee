using System.Diagnostics;

namespace Melodee.Tests.Utility;

public class TimeFormattingTests
{
    [Fact]
    public void ValidateShortTimeFormat()
    {
        var startTicks = Stopwatch.GetTimestamp();
        Thread.Sleep(250);
        var now = DateTimeOffset.UtcNow;
        Assert.NotNull(now.ToString(@"hh\:mm\:ss\.ffff"));
        var formattedTime = Stopwatch.GetElapsedTime(startTicks).ToString(@"hh\:mm\:ss\.ffff");
        Assert.NotNull(formattedTime);
    }
}
