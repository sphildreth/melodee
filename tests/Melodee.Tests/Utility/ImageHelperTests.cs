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
    [InlineData("00-quantic--dancing_while_falling_(deluxe_edition)-web-2024-oma.jpg", false)]
    [InlineData("00-elton_john-never_too_late_soundtrack_to_the_disney_documentary-ost-web-2024.jpg", false)]    
    [InlineData("Band.jpg", true)]
    [InlineData("Artist.jpg", true)]
    [InlineData("Band_1.jpg", true)]
    [InlineData("01-Artist.jpg", true)]
    [InlineData("Band01.jpg", true)]
    [InlineData("Band 01.jpg", true)]
    [InlineData("Band 14.jpg", true)]
    public void FileIsArtistImage(string fileName, bool shouldBe)
    {
        Assert.Equal(ImageHelper.IsArtistImage(new FileInfo(fileName)), shouldBe);
    }

    [Theory]
    [InlineData("logo.jpg", null, false)]
    [InlineData("00-quantic.jpg", null, false)]    
    [InlineData("00-quantic.jpg", "Dancing While Falling", false)]
    [InlineData("front.jpg", null, true)]
    [InlineData("cover.jpg", null, true)]
    [InlineData("01-cover.jpg", null, true)]
    [InlineData("02-cover.jpg", null, true)]
    [InlineData("00-quantic--dancing_while_falling_(deluxe_edition)-web-2024-oma.jpg", "Dancing While Falling", true)]
    [InlineData("Mazzy Star-ghost highway-remastered-CD.jpg", "Ghost Highway", true)]
    [InlineData("Mazzy Star-ghost highway-remastered-booklet-front.jpg", "Ghost Highway", true)]
    public void FileIsCoverImage(string fileName, string? albumName, bool expected) => Assert.Equal(expected, ImageHelper.IsAlbumImage(new FileInfo(fileName), albumName));
}
