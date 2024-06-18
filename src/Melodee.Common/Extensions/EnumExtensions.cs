namespace Melodee.Common.Extensions;

public static class EnumExtensions
{
    public static Dictionary<int, string> ToDictionary(this Enum enumValue)
    {
        var enumType = enumValue.GetType();
        return Enum.GetValues(enumType)
            .Cast<Enum>()
            .ToDictionary(t => (int)(object)t, t => t.ToString());
    }
}