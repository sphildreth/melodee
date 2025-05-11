namespace Melodee.Common.Extensions;

public static class DecimalExtensions
{
    public static string ToStringPadLeft(this decimal input, short padLeft, char padWith = '0')
    {
        return ToStringPadLeft(input as decimal?, padLeft, padWith) ?? string.Empty;
    }

    public static string? ToStringPadLeft(this decimal? input, short padLeft, char padWith = '0')
    {
        if (input == null)
        {
            return null;
        }

        return input.ToString()!.PadLeft(padLeft, padWith);
    }
}
