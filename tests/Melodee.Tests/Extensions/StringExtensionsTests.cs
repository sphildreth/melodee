using System.Runtime.InteropServices;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("Bob", "Bob", true)]
    [InlineData("Bob", "Bob ", true)]
    [InlineData("Bob", "bOb", true)]
    [InlineData("Bob", "BOB ", true)]
    [InlineData("Bob", null, false)]
    [InlineData(null, "Bob", false)]
    [InlineData("Bob", "Steve", false)]
    [InlineData("Steve #1", "Steve #2", false)]
    [InlineData("Steve ^#BOOGER#^", "Steve", false)]
    public void DoStringMatch(string? string1, string? string2, bool shouldBe)
    {
        Assert.Equal(shouldBe, StringExtensions.DoStringsMatch(string1, string2));
    }

    [Theory]
    [InlineData("Bob", "Bob")]
    [InlineData("Bob    ", "Bob")]
    [InlineData("   Bob   ", "Bob")]
    [InlineData("Bob And Nancy", "Bob And Nancy")]
    [InlineData("Bob And Nancy!", "Bob And Nancy!")]
    [InlineData("Bob And Nancy, wITH sTEVE", "Bob And Nancy, With Steve")]
    [InlineData(" Bob    And    Nancy", "Bob And Nancy")]
    [InlineData(" Bob    And    Nancy   ", "Bob And Nancy")]
    [InlineData("With", "With")]
    public void CleanString(string input, string shouldBe)
    {
        Assert.Equal(shouldBe, input.CleanString());
    }

    [Theory]
    [InlineData("Bob", "Bob")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData("Bob ", "Bob")]
    public void Nullify(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input?.Nullify());
    }        
        
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("Bob", null)]
    [InlineData("09/Bob Rocks", null)]        
    [InlineData("Bob Rocks", null)]        
    [InlineData("/Discography 2001-2010/2009/Bob Rocks", 2009)]
    [InlineData("2009/Bob Rocks", 2009)]
    [InlineData("2009 Bob Rocks", 2009)]
    [InlineData("2009", 2009)]
    public void TryToGetYearFromString(string? input, int? shouldBe)
    {
        Assert.Equal(shouldBe, input?.TryToGetYearFromString());
    }
        
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("Bob", null)]
    [InlineData("01 Steve Winwood.mp3", 1)]
    [InlineData(" 01 Steve Winwood.mp3", 1)]
    [InlineData(" 01  - Steve Winwood.mp3", 1)]
    [InlineData("01-Steve Winwood.mp3", 1)]
    [InlineData("01 - Steve Winwood.mp3", 1)]
    [InlineData("001 - Steve Winwood.mp3", 1)]
    [InlineData("14 - Steve Winwood.mp3", 14)]
    [InlineData(" 01 - Steve Winwood.mp3", 1)]
    public void TryToGetSongNumberFromString(string? input, int? shouldBe)
    {
        Assert.Equal(shouldBe, input?.TryToGetSongNumberFromString());
    }    
        
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("Bob", "Bob")]
    [InlineData("01 Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData(" 01 Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData(" 01  - Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData("01-Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData("01 - Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData("001 - Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData("14 - Steve Winwood.mp3", "Steve Winwood.mp3")]
    [InlineData(" 01 - Steve Winwood.mp3", "Steve Winwood.mp3")]
    public void RemoveSongNumberFromString(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input?.RemoveSongNumberFromString());
    }
        
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
        Assert.Equal(shouldBe, input.IsVariousArtistValue());
    }
        
    [Theory]
    [InlineData("Bob Jones", false)]
    [InlineData("Bob Song", false)]
    [InlineData("Song Bob", false)]
    [InlineData("Sound Bob", false)]
    [InlineData("SoundSongs", true)]
    [InlineData("Sound Song", true)]
    [InlineData("Sound Songs", true)]
    public void ValidateIsSoundSongArtists(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, input.IsSoundSongAristValue());
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
        Assert.Equal(shouldBe, input.IsCastRecording());
    }        
        
    [Theory]
    [InlineData(null, false)]
    [InlineData("Something", false)]
    [InlineData("Eternally Gifted", false)]
    [InlineData("Shift Scene", false)]
    [InlineData("Something Ft Bob", true)]
    [InlineData("Something ft Bob", true)]
    [InlineData("Something Ft. Bob", true)]
    [InlineData("Something (Ft. Bob)", true)]
    [InlineData("Something Feat. Bob", true)]
    [InlineData("Something Featuring Bob", true)]
    [InlineData("Ariana Grande ft Nonna", true)]
    public void StringHasFeaturingFragments(string? input, bool shouldBe)
    {
        Assert.Equal(shouldBe, input.HasFeaturingFragments());
    }
    
    [Theory]
    [InlineData(null, false)]
    [InlineData("Something With Bob", false)]
    [InlineData("Something (with Bob)", true)]
    [InlineData("Minds Without Fear with Vishal-Shekhar", true)]
    public void StringHasWithFragments(string? input, bool shouldBe)
    {
        Assert.Equal(shouldBe, input.HasWithFragments());
    }    

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Something With Bob", "Something With Bob")]
    [InlineData("Something\ufffdWith Bob=", "Something With Bob")]
    [InlineData("       \ufffd   \ufffd\ufffd\ufffd   Artist....: Holy Truth                          \ufffd\ufffd\ufffd", "Artist....: Holy Truth")]
    public void StringOnlyAlphanumeric(string? input, string? shouldBe) => Assert.Equal(shouldBe, input.OnlyAlphaNumeric());

    [Theory]
    [InlineData("00-pixel_-_reality_strikes_back-2004-mycel.sfv", 2004)]
    public void ValidateParsingYearFromFileName(string input, int? shouldBe) => Assert.Equal(shouldBe, input.TryToGetYearFromString());
        
    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Bob Jones", "Bob Jones")]
    [InlineData("Bob Jones; Artist Name", "Bob Jones/Artist Name")]
    [InlineData(" Bob Jones;   Artist Name ", "Bob Jones/Artist Name")]
    [InlineData("; Bob Jones;;Artist Name", "Bob Jones/Artist Name")]
    public void ValidateToCleanedMultipleArtistsValue(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input.ToCleanedMultipleArtistsValue());
    }        
}
