using System.ComponentModel;
using System.Reflection;

namespace Melodee.Common.Extensions;

public static class EnumExtensions
{
    public static string GetEnumDescriptionValue<T>(this T @enum) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new InvalidOperationException();
        }

        var name = @enum.ToString() ?? string.Empty;
        return typeof(T).GetField(name)!.GetCustomAttribute<DescriptionAttribute>(false)?.Description ?? name;
    }

    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        var enumType = value.GetType();
        var name = Enum.GetName(enumType, value) ?? string.Empty;
        return enumType.GetField(name)!.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
    }

    public static Dictionary<int, string> ToDictionary(this Enum enumValue)
    {
        var enumType = enumValue.GetType();
        return Enum.GetValues(enumType)
            .Cast<Enum>()
            .ToDictionary(t => (int)(object)t, t => t.ToString());
    }
}
