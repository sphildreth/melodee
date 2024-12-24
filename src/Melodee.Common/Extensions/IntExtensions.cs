namespace Melodee.Common.Extensions;

public static class IntExtensions
{
    public static bool AreNumbersSequential(this int[] array)
    {
        if (array.Length == 0)
        {
            return false;
        }
        if (array.Length == 1)
        {
            return true;
        }
        return array.Zip(array.Skip(1), (a, b) => (a + 1) == b).All(x => x);
    }    
    
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
