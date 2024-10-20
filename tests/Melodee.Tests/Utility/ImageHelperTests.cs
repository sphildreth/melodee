using Melodee.Common.Utility;

namespace Melodee.Tests.Utility;

public sealed class ImageHelperTests
{
    [Theory]
    [InlineData("info.txt", false)]
    [InlineData("band info.txt", false)]
    [InlineData("band.txt", false)]
    [InlineData("Something With Bob.jpg", false)]
    [InlineData("logo.jpg", false)]
    [InlineData("Band.jpg", true)]
    [InlineData("Band_1.jpg", true)]
    [InlineData("Band01.jpg", true)]
    [InlineData("Band 01.jpg", true)]
    [InlineData("Band 14.jpg", true)]
    public void FileIsBandImage(string fileName, bool shouldBe)
    {
        Assert.Equal(ImageHelper.IsArtistImage(new FileInfo(fileName)), shouldBe);
    }
}