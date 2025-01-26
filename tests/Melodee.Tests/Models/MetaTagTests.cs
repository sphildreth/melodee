using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Tests.Models;

public class MetaTagTests
{
    [Fact]
    public void ValidateWasModified()
    {
        var tag = new MetaTag<object?>
        {
            OriginalValue = "Test",
            Value = "Test",
            Identifier = MetaTagIdentifier.Album
        };
        Assert.False(tag.WasModified);

        var tag2 = new MetaTag<object?>
        {
            OriginalValue = "Tes1",
            Value = "Test2",
            Identifier = MetaTagIdentifier.Album
        };
        Assert.True(tag2.WasModified);
    }
}
