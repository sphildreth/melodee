namespace Melodee.Common.Extensions;

public static class EnumerableExtensions
{
    public static string ToCsv<T>(this IEnumerable<T> source)
    {
        return string.Join(',', source);
    }

    public static string ToDelimitedList<T>(this IEnumerable<T> source, char delimiter = '|')
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        return string.Join(delimiter, source.Select(s => s?.ToString()).ToArray());
    }

    public static IEnumerable<string>? FromDelimitedList(this string? csvList, bool nullOrWhitespaceInputReturnsNull = false, char delimiter = '|')
    {
        if (string.IsNullOrWhiteSpace(csvList))
        {
            return nullOrWhitespaceInputReturnsNull ? null : [];
        }

        return csvList
            .TrimEnd(',')
            .Split(',')
            .AsEnumerable<string>()
            .Select(s => s.Trim())
            .ToList();
    }

    public static void ForEach<T>(this IEnumerable<T> ie, Action<T, int> action)
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
