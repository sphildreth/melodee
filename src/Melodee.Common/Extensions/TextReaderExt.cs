namespace Melodee.Common.Extensions
{
    public static class TextReaderExt
    {
        public static IEnumerable<string> ReadAllLines(this TextReader textReader)
        {
            var lines = new List<string>();

            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}
