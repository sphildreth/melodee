namespace Melodee.Common.Extensions;

public static class EnumerableExtensions
{
    public static string ToCsv<T>(this IEnumerable<T> source, string join = ",")
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        return string.Join(join, source.Select(s => s?.ToString()).ToArray());
    }

    public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
    {
        var i = 0;
        foreach (var e in ie)
        {
            action(e, i++);
        }
    }

    public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T>? source, Func<T, IEnumerable<T>> selector)
    {
        var result = source?.SelectMany(selector) ?? [];
        if (!result.Any())
        {
            return result;
        }

        return (result ?? Array.Empty<T>()).Concat(result.SelectManyRecursive(selector));
    }
}
