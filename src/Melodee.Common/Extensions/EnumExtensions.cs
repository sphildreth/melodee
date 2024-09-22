using System.ComponentModel;
using System.Reflection;

namespace Melodee.Common.Extensions;

public static class EnumExtensions
{
    public static string GetEnumDescriptionValue<T>(this T @enum) where T : struct
    {
        if(!typeof(T).IsEnum)
            throw new InvalidOperationException();

        return typeof(T).GetField(@enum.ToString()).GetCustomAttribute<DescriptionAttribute>(false).Description;
    }
    
    public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        var enumType = value.GetType();
        var name = Enum.GetName(enumType, value);
        return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
    }    
    
    public static Dictionary<int, string> ToDictionary(this Enum enumValue)
    {
        var enumType = enumValue.GetType();
        return Enum.GetValues(enumType)
            .Cast<Enum>()
            .ToDictionary(t => (int)(object)t, t => t.ToString());
    }
}
