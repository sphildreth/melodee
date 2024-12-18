﻿using Melodee.Common.Utility;
using Melodee.Plugins.Validation;

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
    [InlineData("001.jpg", false)]
    [InlineData("Band.jpg", true)]
    [InlineData("Artist.jpg", true)]
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
    public void FileIsArtistSecondaryImage(string fileName, bool shouldBe)
    {
        Assert.Equal(ImageHelper.IsArtistSecondaryImage(new FileInfo(fileName)), shouldBe);
    }    

    [Theory]
    [InlineData("logo.jpg", null, false)]
    [InlineData("00-quantic.jpg", null, false)]    
    [InlineData("00-quantic.jpg", "Dancing While Falling", false)]
    [InlineData("Booklet2-Scans.jpg", null, false)]
    [InlineData("Scan_06-Covers.jpg", null, false)]
    [InlineData("Back-Scans.jpg", null, false)]
    [InlineData("02-cover.jpg", null, false)]    
    [InlineData("Front-Scans.jpg", null, true)]    
    [InlineData("front.jpg", null, true)]
    [InlineData("cover.jpg", null, true)]
    [InlineData("01-cover.jpg", null, true)]
    [InlineData("Covers-001.jpg", null, true)]
    [InlineData("Scan_01-Covers.jpg", null, true)]
    [InlineData("00-quantic--dancing_while_falling_(deluxe_edition)-web-2024-oma.jpg", "Dancing While Falling", true)]
    [InlineData("Mazzy Star-ghost highway-remastered-CD.jpg", "Ghost Highway", true)]
    [InlineData("Mazzy Star-ghost highway-remastered-booklet-front.jpg", "Ghost Highway", true)]
    public void FileIsAlbumImage(string fileName, string? albumName, bool expected) => Assert.Equal(expected, ImageHelper.IsAlbumImage(new FileInfo(fileName), albumName));
    
    [Theory]
    [InlineData("logo.jpg",  false)]
    [InlineData("00-quantic.jpg",  false)]    
    [InlineData("front.jpg",  false)]
    [InlineData("cover.jpg",  false)]
    [InlineData("01-cover.jpg",  false)]
    [InlineData("Front-Scans.jpg",  false)]
    [InlineData("CD-Scans.jpg",  false)]
    [InlineData("img001-Scans.jpg",  false)]
    [InlineData("02-cover.jpg",  true)]    
    [InlineData("Back-Scans.jpg",  true)]
    [InlineData("Booklet2-Scans.jpg",  true)]
    [InlineData("img005-Scans.jpg",  true)]
    [InlineData("Scan_06-Covers.jpg", true)]
    [InlineData("Covers-002.jpg", true)]
    public void FileIsAlbumSecondaryImage(string fileName, bool expected) => Assert.Equal(expected, ImageHelper.IsAlbumSecondaryImage(new FileInfo(fileName)));    
    
    [Theory]
    [InlineData("logo.jpg",  false)]
    [InlineData("00-quantic.jpg",  false)]    
    [InlineData("front.jpg",  false)]
    [InlineData("cover.jpg",  false)]
    [InlineData("01-cover.jpg",  false)]
    [InlineData("Front-Scans.jpg",  false)]
    [InlineData("CD-Scans.jpg",  false)]
    [InlineData("img001-Scans.jpg",  false)]
    [InlineData("bob-proof.jpg",  true)]    
    [InlineData("proof.jpg",  true)]
    [InlineData("foto.jpg",  true)]
    [InlineData("foto1.jpg",  true)]
    [InlineData("foto2.jpg", true)]
    public void FileIsProofImage(string fileName, bool expected) => Assert.Equal(expected, ImageValidator.IsImageAProofType(fileName));     
}
