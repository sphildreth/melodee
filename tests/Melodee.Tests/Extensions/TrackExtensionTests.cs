using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class TrackExtensionTests
{
    [Fact]
    public void ValidateTrackNewFileName()
    {
        var release = ReleaseExtensionTests.NewRelease();
        var trackNewFileName = release.Tracks!.First().ToTrackFileName();
        Assert.NotNull(trackNewFileName);
        Assert.Equal(@"003 Flako El Dark Cowboy.mp3", trackNewFileName);
    }
}