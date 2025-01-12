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
        Assert.Equal(shouldBe, string1.DoStringsMatch(string2));
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
    [InlineData("\\0 Goofy (C)\\x00 Doofies\u2400\\u0000", "Goofy (C) Doofies")]
    [InlineData("With", "With")]
    [InlineData("Show Me \u0026 Wrong", "Show Me & Wrong")]
    public void CleanString(string input, string shouldBe)
    {
        Assert.Equal(shouldBe, input.CleanString());
    }

    [Theory]
    [InlineData("Bob", "Bob")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData("null", null)]
    [InlineData("null ", null)]
    [InlineData("  NULL ", null)]
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
    [InlineData("Bob", null)]
    [InlineData("Bobs Discography", null)]
    [InlineData("Media Madness", null)]
    [InlineData("Media 2013 Madness", null)]
    [InlineData("Artist - [2000] Album", null)]
    [InlineData("CD", null)]
    [InlineData("A0", 1)] // sph; I dont know why, but these are common
    [InlineData("A1", 1)]
    [InlineData("AA1", 1)]
    [InlineData("AAA1", 1)]
    [InlineData("B2", 2)]
    [InlineData("CD01", 1)]
    [InlineData("CD1", 1)]
    [InlineData("CD4", 4)]
    [InlineData("CD04", 4)]
    [InlineData("CD004", 4)]
    [InlineData("DISC1", 1)]
    [InlineData("media001", 1)]
    public void TryToGetMediaNumberFromString(string? input, int? shouldBe)
    {
        Assert.Equal(shouldBe, input?.TryToGetMediaNumberFromString());
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
    public void StringOnlyAlphanumeric(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input.OnlyAlphaNumeric());
    }

    [Theory]
    [InlineData("00-pixel_-_reality_strikes_back-2004-mycel.sfv", 2004)]
    public void ValidateParsingYearFromFileName(string input, int? shouldBe)
    {
        Assert.Equal(shouldBe, input.TryToGetYearFromString());
    }

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


    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("Bob", "BOB")]
    [InlineData("Bob Jones ", "BOBJONES")]
    [InlineData("Bob JONEs ", "BOBJONES")]
    [InlineData("Bob JONEs and The 'secret' five!", "BOBJONESANDTHESECRETFIVE")]
    [InlineData("\\0 Goofy (C)\\x00 Doofies\u2400\\u0000", "GOOFYCDOOFIES")]
    public void ValidateNormalizedString(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input.ToNormalizedString());
    }

    [Fact]
    public void ValidateNormalizedLanguageStrings()
    {
        //d835dd1fd835dd29d835dd2cd835dd2cd835dd212665fe0fd835dd30d835dd31d835dd1ed835dd26d835dd2bd835dd22d835dd21306f3055307f
        //8J2Un/CdlKnwnZSs8J2UrPCdlKHimaXvuI/wnZSw8J2UsfCdlJ7wnZSm8J2Uq/CdlKLwnZSh44Gv44GV44G/
        var test = "𝔟𝔩𝔬𝔬𝔡♥️𝔰𝔱𝔞𝔦𝔫𝔢𝔡はさみ";
        var testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);
        var testNormalized2 = test.ToNormalizedString();
        Assert.Equal(testNormalized, testNormalized2);

        test = "            انا خايف اكرهك";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);
        Assert.NotEqual(test, testNormalized2);

        test = "        بسحرولك";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);

        test = "タクト";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);

        test = "مهيرة السودان";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);

        test = "千のナイフ";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);

        test = "今天再生";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);

        test = "မြဲနေသေးတဲ့လက်တွဲတစ်ခု";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);

        test = "ジューシィ・フルーツ ヒットコレクション";
        Assert.NotEqual(test, test.ToNormalizedString());
        testNormalized = test.ToNormalizedString();
        Assert.NotNull(testNormalized);
        Assert.NotEqual(test, testNormalized);
    }


    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("enc:", null)]
    [InlineData("enc:null", null)]
    [InlineData("736573616d65", "sesame")]
    [InlineData("enc:736573616d65", "sesame")]
    [InlineData("736F6D657468696E675F6C6F6E675F235F2B215F776974685F2E782D642A5F77656972645F63686172616374657273", "something_long_#_+!_with_.x-d*_weird_characters")]
    public void ValidateHexDecoding(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input?.FromHexString());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("sesame", "736573616D65")]
    [InlineData("something_long_#_+!_with_.x-d*_weird_characters", "736F6D657468696E675F6C6F6E675F235F2B215F776974685F2E782D642A5F77656972645F63686172616374657273")]
    public void ValidateHexEncoding(string? input, string? shouldBe)
    {
        Assert.Equal(shouldBe, input?.ToHexString());
    }

    [Fact]
    public void ValidateTags()
    {
        var tags = new[] { "tag1", "TAG2", "tag3", "tag4", "tag5", "Tag6" };
        var tag = "".AddTag(tags);
        Assert.NotNull(tag);
        Assert.Contains("tag2", tag.ToTags() ?? throw new InvalidOperationException());

        var tagNoLower = "".AddTag(tags, dontLowerCase: true);
        Assert.NotNull(tagNoLower);
        Assert.Contains("TAG2", tagNoLower.ToTags() ?? throw new InvalidOperationException());
    }

    [Theory]
    [InlineData(null, 0, null)]
    [InlineData(null, 6, null)]
    [InlineData("", 0, null)]
    [InlineData("1234", 4, "1234")]
    [InlineData("12345", 4, "1234")]
    [InlineData("12345", 2, "12")]
    [InlineData("12345", 999, "12345")]
    public void ValidateTruncateLength(string? input, int length, string? shouldBe)
    {
        Assert.Equal(shouldBe, input?.TruncateLongString(length));
    }
}
