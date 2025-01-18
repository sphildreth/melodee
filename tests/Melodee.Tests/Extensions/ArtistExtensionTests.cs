using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class ArtistExtensionTests
{
    [Theory]
    [InlineData("Bob Jones", false)]
    [InlineData("Bob Various", false)]
    [InlineData("Various Bob", false)]
    [InlineData("VA", true)]
    [InlineData("[VA]", true)]
    [InlineData("various artists", true)]
    [InlineData("Various Artists", true)]
    [InlineData("[Various Artists]", true)]
    [InlineData("VARIOUS ARTISTS", true)]
    public void ValidateIsVariousArtists(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, Artist.NewArtistFromName(input).IsVariousArtist());
    }

    [Theory]
    [InlineData("Bob Jones", false)]
    [InlineData("Bob Cast", false)]
    [InlineData("Song Bob", false)]
    [InlineData("Sound Bob", false)]
    [InlineData("Original Cast", true)]
    [InlineData("Original Broadway Cast", true)]
    public void ValidateIsCastRecordSongArtists(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, Artist.NewArtistFromName(input).IsCastRecording());
    }
}
