namespace Melodee.Common.Extensions;

public static class ShortExtensions
{
    public static string ToStringPadLeft(this short input, short padLeft, char padWith = '0')
    {
        return ToStringPadLeft(input as short?, padLeft, padWith) ?? string.Empty;
    }

    public static string? ToStringPadLeft(this short? input, short padLeft, char padWith = '0')
    {
        if (input == null)
        {
            return null;
        }

        return input.ToString()!.PadLeft(padLeft, padWith);
    }
}
