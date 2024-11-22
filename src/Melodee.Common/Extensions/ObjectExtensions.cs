using System.Collections;
using System.Reflection;
using Melodee.Common.Exceptions.Attributes;

namespace Melodee.Common.Extensions;

public static class ObjectExtensions
{
    public static bool IsEnumerable(this object obj)
        => obj is IEnumerable;    
    
    
    
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
    
    public static object? Convert(this object? value, Type t)
    {
        Type? underlyingType = Nullable.GetUnderlyingType(t);
        if (underlyingType != null && value == null)
        {
            return null;
        }
        var basetype = underlyingType ?? t;
        return System.Convert.ChangeType(value, basetype);
    }

    public static T? Convert<T>(this object? value)
    {
        if (value is not IConvertible)
        {
            return (T?)value;
        }
        return value == null ? default : (T)value.Convert(typeof(T))!;
    }    
    
    public static bool IsNumericType(this object o)
    {   
        switch (Type.GetTypeCode(o.GetType()))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
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
