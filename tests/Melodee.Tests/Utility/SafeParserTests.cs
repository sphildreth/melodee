using Melodee.Common.Utility;

namespace Melodee.Tests.Utility
{
    public sealed class SafeParserTests
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
    }
}
