using Melodee.Common.Extensions;

namespace Melodee.Tests.Extensions;

public class IntExtensionTests
{
    [Fact]
    public void ValidateIntArraysAreSequential()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };
        Assert.True(numbers.AreNumbersSequential());

        numbers = new[] { 1, 4, 3, 9, 5 };
        Assert.False(numbers.AreNumbersSequential());

        numbers = new[] { 1, 2, 3, 3, 4, 5 };
        Assert.False(numbers.AreNumbersSequential());

        numbers = new[] { 1 };
        Assert.True(numbers.AreNumbersSequential());
    }
}
