using Melodee.Common.Extensions;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Tests.Models.SearchEngine;

public class QueryModelTests
{
    [Fact]
    public void ValidateQueryModelReversesProperly()
    {
        var name = "John Waite";
        var query = new Query
        {
            Name = name
        };
        Assert.Equal(name.ToNormalizedString(), query.NameNormalized);
        Assert.Equal("WAITEJOHN", query.NameNormalizedReversed);
    }

    [Fact]
    public void ValidateQueryModelReversesProperlyWithComma()
    {
        var name = "Waite, John";
        var query = new Query
        {
            Name = name
        };
        Assert.Equal(name.ToNormalizedString(), query.NameNormalized);
        Assert.Equal("JOHNWAITE", query.NameNormalizedReversed);
    }
}
