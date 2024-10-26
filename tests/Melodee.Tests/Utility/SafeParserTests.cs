using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Moq;
using Serilog;

namespace Melodee.Tests.Utility;

public sealed class SafeParserTests : TestsBase
{
    [Theory]        
    [InlineData("02/22/1988")]
    [InlineData("02/22/88")]
    [InlineData("02-22-1988")]
    [InlineData("1988")]
    [InlineData("\"1988\"")]
    [InlineData("1988-06-15T07:00:00Z")]
    [InlineData("1988/05/02")]
    public void DateFromString(string input)
    {
        Assert.NotNull(SafeParser.ToDateTime(input));
    }

    [Fact]
    public void ValidateToHash()
    {
        Assert.True(SafeParser.Hash("Bob", "Marley") > 0);
            
        Assert.True(SafeParser.Hash("Bob", "Marley", "") > 0);
            
        Assert.True(SafeParser.Hash("Bob", "Marley", "", null) > 0);

        string? nothing = null;
        Assert.False(SafeParser.Hash(nothing) > 0);
    }
    
    [Fact]
    public void FromSerializedJsonArrayToCharArray()
    {
        var data = "['^', '~', '#']";
        var strings = MelodeeConfiguration.FromSerializedJsonArray(data, new Serializer(new Mock<ILogger>().Object));
        Assert.NotNull(strings);
        Assert.NotEmpty(strings);
        Assert.Contains("^", strings);
        
        
    }

    [Fact]
    public void FromSerializedJsonDictionary()
    {
        var configuration = TestsBase.NewConfiguration();
        var artistReplacement = MelodeeConfiguration.FromSerializedJsonDictionary(configuration[SettingRegistry.ProcessingArtistNameReplacements], Serializer);
        Assert.NotNull(artistReplacement);
        Assert.NotEmpty(artistReplacement);
        Assert.Contains(artistReplacement, x => x.Key == "AC/DC");
    }
}
