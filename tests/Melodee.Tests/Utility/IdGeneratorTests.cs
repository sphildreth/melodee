using Melodee.Common.Utility;

namespace Melodee.Tests.Utility;

public class IdGeneratorTests
{
    [Fact]
    public void ValidateIdGeneratorIsUnique()
    {
        var id = IdGenerator.CreateId;
        Assert.True(id > 0);
        var id2 = IdGenerator.CreateId;
        Assert.True(id2 > 0);
        Assert.NotEqual(id, id2);
    }
}
