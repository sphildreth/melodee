namespace Melodee.Common.Extensions;

public static class IntExtensions
{
    public static string ToStringPadLeft(this int input, short padLeft, char padWith = '0')
    {
        return ToStringPadLeft(input as int?, padLeft, padWith) ?? string.Empty;
    }

    public static string? ToStringPadLeft(this int? input, short padLeft, char padWith = '0')
    {
        if (input == null)
        {
            return null;
        }

        return input!.ToString()!.PadLeft(padLeft, padWith);
    }
}
