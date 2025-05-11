namespace Melodee.Common.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();
        foreach (var dict in dictionaries)
        foreach (var x in dict)
        {
            result[x.Key] = x.Value;
        }

        return result;
    }
}
