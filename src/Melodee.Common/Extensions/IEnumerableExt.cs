namespace Melodee.Common.Extensions
{
    public static class IEnumerableExt
    {
        public static string ToCsv<T>(this IEnumerable<T> source, string join = ",")
        {
            if (source == null) throw new ArgumentNullException("source");
            return string.Join(join, source.Select(s => s.ToString()).ToArray());
        }
    }
}
