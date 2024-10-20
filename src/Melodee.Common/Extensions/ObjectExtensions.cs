using System.Reflection;
using Melodee.Common.Exceptions.Attributes;

namespace Melodee.Common.Extensions;

public static class ObjectExtensions
{
    public static IDictionary<string, object?> ToDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        var result = new Dictionary<string, object?>();
        foreach (var propInfo in source.GetType().GetProperties(bindingAttr))
        {
            var keyName = propInfo.Name;
            var melodeeDictionaryOptionsAttribute = propInfo.GetCustomAttribute<MelodeeDictionaryOptionsAttribute>();
            if (melodeeDictionaryOptionsAttribute != null)
            {
                if (melodeeDictionaryOptionsAttribute.Ignore)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(melodeeDictionaryOptionsAttribute.Key))
                {
                    keyName = melodeeDictionaryOptionsAttribute.Key!;
                }
            }

            try
            {
                result[keyName] = propInfo.GetValue(source, null);
            }
            catch
            {
                // ignored
            }
        }

        return result;
    }

    public static bool IsNullOrDefault<T>(this T argument)
    {
        if (argument == null)
        {
            return true;
        }

        if (Equals(argument, default(T)))
        {
            return true;
        }

        var methodType = typeof(T);
        if (Nullable.GetUnderlyingType(methodType) != null)
        {
            return false;
        }

        var argumentType = argument.GetType();
        if (argumentType.IsValueType && argumentType != methodType)
        {
            var obj = Activator.CreateInstance(argument.GetType());
            return obj?.Equals(argument) ?? false;
        }

        return false;
    }
}
