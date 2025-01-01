namespace Melodee.Common.Extensions;

public static class BoolExtensions
{
    public static string ToLowerCaseString(this bool value)
    {
        return value.ToString().ToLower();
    }
}
