namespace Melodee.Common.Extensions;

public static class DoubleExtensions
{
    public static int ToSeconds(this double seconds) => Convert.ToInt32(seconds * 1000);
}
