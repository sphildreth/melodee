using Melodee.Common.Utility;

namespace Melodee.Tests.Plugins.Discovery;

public class FileHelperTests
{
    [Theory]
    [InlineData("sfv", true)]
    [InlineData("SFV", true)]
    [InlineData("Sfv", true)]
    [InlineData(".sfv", true)]
    [InlineData("mp3", false)]
    [InlineData("mxx", false)]
    [InlineData("txt", false)]
    [InlineData("jpg", false)]
    [InlineData("png", false)]
    public void ValidateIsMediaMetaDataTypeFile(string extension, bool shouldBe)
    {
        Assert.Equal(shouldBe, FileHelper.IsFileMediaMetaDataType(extension));
    }

    [Theory]
    [InlineData("mp3", true)]
    [InlineData("MP3", true)]
    [InlineData("Mp3", true)]
    [InlineData(".mp3", true)]
    [InlineData("m3u", false)]
    [InlineData("sfv", false)]
    [InlineData("mxx", false)]
    [InlineData("txt", false)]
    [InlineData("jpg", false)]
    [InlineData("png", false)]
    public void ValidateIsMediaTypeFile(string extension, bool shouldBe)
    {
        Assert.Equal(shouldBe, FileHelper.IsFileMediaType(extension));
    }

    [Theory]
    [InlineData("jpg", true)]
    [InlineData("JPG", true)]
    [InlineData("Jpg", true)]
    [InlineData(".jpg", true)]
    [InlineData("png", true)]
    [InlineData("webp", true)]
    [InlineData("gif", true)]
    [InlineData("mp3", false)]
    [InlineData("sfv", false)]
    [InlineData("mxx", false)]
    [InlineData("txt", false)]
    public void ValidateIsImageFileTypeFile(string extension, bool shouldBe)
    {
        Assert.Equal(shouldBe, FileHelper.IsFileImageType(extension));
    }
}
