using Melodee.Common.Plugins.Validation;
using Melodee.Common.Utility;

namespace Melodee.Tests.Utility;

public sealed class ImageHelperTests
{
    [Theory]
    [InlineData("info.txt", false)]
    [InlineData("band info.txt", false)]
    [InlineData("band.txt", false)]
    [InlineData("Something With Bob.jpg", false)]
    [InlineData("00-quantic--dancing_while_falling_(deluxe_edition)-web-2024-oma.jpg", false)]
    [InlineData("00-elton_john-never_too_late_soundtrack_to_the_disney_documentary-ost-web-2024.jpg", false)]
    [InlineData("Band_2.jpg", false)]
    [InlineData("Band 14.jpg", false)]
    [InlineData("artist 07.jpg", false)]
    [InlineData("001.jpg", false)]
    [InlineData("Band.jpg", true)]
    [InlineData("Artist.jpg", true)]
    [InlineData("artist 01.jpg", true)]
    [InlineData("artist 00.jpg", true)]
    [InlineData("artist_01.jpg", true)]
    [InlineData("artist_1.jpg", true)]
    [InlineData("Band_1.jpg", true)]
    [InlineData("01-Artist.jpg", true)]
    [InlineData("Band01.jpg", true)]
    [InlineData("Band0001.jpg", true)]
    [InlineData("Band-001.jpg", true)]
    [InlineData("Band 01.jpg", true)]
    public void FileIsArtistImage(string fileName, bool shouldBe)
    {
        Assert.Equal(ImageHelper.IsArtistImage(new FileInfo(fileName)), shouldBe);
    }

    [Theory]
    [InlineData("info.txt", false)]
    [InlineData("band info.txt", false)]
    [InlineData("band.txt", false)]
    [InlineData("Something With Bob.jpg", false)]
    [InlineData("00-quantic--dancing_while_falling_(deluxe_edition)-web-2024-oma.jpg", false)]
    [InlineData("00-elton_john-never_too_late_soundtrack_to_the_disney_documentary-ost-web-2024.jpg", false)]
    [InlineData("Band.jpg", false)]
    [InlineData("Artist.jpg", false)]
    [InlineData("Band_1.jpg", false)]
    [InlineData("01-Artist.jpg", false)]
    [InlineData("Band01.jpg", false)]
    [InlineData("Band 01.jpg", false)]
    [InlineData("Band 02.jpg", true)]
    [InlineData("Band 14.jpg", true)]
    [InlineData("logo.jpg", true)]
    [InlineData("artist 07.jpg", true)]
    public void FileIsArtistSecondaryImage(string fileName, bool shouldBe)
    {
        Assert.Equal(ImageHelper.IsArtistSecondaryImage(new FileInfo(fileName)), shouldBe);
    }

    [Theory]
    [InlineData("logo.jpg", false)]
    [InlineData("00-quantic.jpg", false)]
    [InlineData("Booklet2-Scans.jpg", false)]
    [InlineData("Scan_06-Covers.jpg", false)]
    [InlineData("Back-Scans.jpg", false)]
    [InlineData("Back Cover.jpg", false)]
    [InlineData("Inner Cover.jpg", false)]
    [InlineData("Proof.jpg", false)]
    [InlineData("mY nAME is BOB & here iS my proof image.jpg", false)]
    [InlineData("02-cover.jpg", false)]
    [InlineData("artist.jpg", false)]
    [InlineData("artist 000.jpg", false)]
    [InlineData("artist 1.jpg", false)]
    [InlineData("artist 07.jpg", false)]
    [InlineData("Mazzy Star-ghost highway-remastered-CD.jpg", false)]
    [InlineData("Mazzy Star-ghost highway-remastered-booklet-front.jpg", false)]
    [InlineData("release 10.jpg", false)]
    [InlineData("3-4.jpg", false)]
    [InlineData("00-002.jpg", false)]
    [InlineData("Front-Scans.jpg", true)]
    [InlineData("Front Cover.jpg", true)]
    [InlineData("front.jpg", true)]
    [InlineData("cover.jpg", true)]
    [InlineData("TG_front.jpg", true)]
    [InlineData("01-cover.jpg", true)]
    [InlineData("release 00.jpg", true)]
    [InlineData("Covers-001.jpg", true)]
    [InlineData("Scan_01-Covers.jpg", true)]
    [InlineData("Kim Wilde - You Came (2006) (Cover).jpg", true)]
    [InlineData("folder.jpg", true)]
    [InlineData("1folder.jpg", true)]
    [InlineData("1.folder.jpg", true)]
    [InlineData("00-001.jpg", true)]
    public void FileIsAlbumImage(string fileName, bool expected)
    {
        Assert.Equal(expected, ImageHelper.IsAlbumImage(new FileInfo(fileName)));
    }

    [Theory]
    [InlineData("logo.jpg", false)]
    [InlineData("00-quantic.jpg", false)]
    [InlineData("front.jpg", false)]
    [InlineData("cover.jpg", false)]
    [InlineData("01-cover.jpg", false)]
    [InlineData("Front-Scans.jpg", false)]
    [InlineData("img001-Scans.jpg", false)]
    [InlineData("Kim Wilde - You Came (2006) (Cover).jpg", false)]
    [InlineData("artist.jpg", false)]
    [InlineData("artist 000.jpg", false)]
    [InlineData("artist 1.jpg", false)]
    [InlineData("artist 07.jpg", false)]
    [InlineData("release 00.jpg", false)]
    [InlineData("00-001.jpg", false)]
    [InlineData("release 10.jpg", true)]
    [InlineData("CD-Scans.jpg", true)]
    [InlineData("02-cover.jpg", true)]
    [InlineData("Back-Scans.jpg", true)]
    [InlineData("Back Cover.jpg", true)]
    [InlineData("Inner Cover.jpg", true)]
    [InlineData("Booklet2-Scans.jpg", true)]
    [InlineData("img005-Scans.jpg", true)]
    [InlineData("Scan_06-Covers.jpg", true)]
    [InlineData("Covers-002.jpg", true)]
    [InlineData("Mazzy Star-ghost highway-remastered-CD.jpg", true)]
    [InlineData("Mazzy Star-ghost highway-remastered-booklet-front.jpg", true)]
    [InlineData("3-4.jpg", true)]
    [InlineData("back.jpg", true)]
    [InlineData("cd.jpg", true)]
    [InlineData("1-2 FRONT-INLAY.jpg", true)]
    [InlineData("00-002.jpg", true)]
    [InlineData("00-003.jpg", true)]
    public void FileIsAlbumSecondaryImage(string fileName, bool expected)
    {
        Assert.Equal(expected, ImageHelper.IsAlbumSecondaryImage(new FileInfo(fileName)));
    }

    [Fact]
    public void ValidateImageFileNameParts()
    {
        var parts = "Back Cover.jpg";
        var pp = ImageHelper.NormalizedImageTypesFromFilename(parts);
        Assert.NotNull(pp);
        Assert.Contains(pp, x => x == "COVER");
        Assert.True(pp.Length == 1);

        parts = "Proof.jpg";
        pp = ImageHelper.NormalizedImageTypesFromFilename(parts);
        Assert.Null(pp);

        parts = "Cover.jpg";
        pp = ImageHelper.NormalizedImageTypesFromFilename(parts);
        Assert.NotNull(pp);
        Assert.Contains(pp, x => x == "COVER");
        Assert.True(pp.Length == 1);

        parts = "Artist 01.jpg";
        pp = ImageHelper.NormalizedImageTypesFromFilename(parts);
        Assert.NotNull(pp);
        Assert.Contains(pp, x => x == "ARTIST");
        Assert.True(pp.Length == 1);
    }

    [Theory]
    [InlineData("logo.jpg", false)]
    [InlineData("00-quantic.jpg", false)]
    [InlineData("front.jpg", false)]
    [InlineData("cover.jpg", false)]
    [InlineData("01-cover.jpg", false)]
    [InlineData("Front-Scans.jpg", false)]
    [InlineData("CD-Scans.jpg", false)]
    [InlineData("img001-Scans.jpg", false)]
    [InlineData("bob-proof.jpg", true)]
    [InlineData("proof.jpg", true)]
    [InlineData("foto.jpg", true)]
    [InlineData("foto1.jpg", true)]
    [InlineData("foto2.jpg", true)]
    public void FileIsProofImage(string fileName, bool expected)
    {
        Assert.Equal(expected, ImageValidator.IsImageAProofType(fileName));
    }
}
