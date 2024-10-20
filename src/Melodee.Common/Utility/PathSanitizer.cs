using System.Text;

namespace Melodee.Common.Utility;

public static class PathSanitizer
{
    /// <summary>
    ///     The set of invalid filename characters, kept sorted for fast binary search
    /// </summary>
    private static readonly char[] InvalidFilenameChars;

    /// <summary>
    ///     The set of invalid path characters, kept sorted for fast binary search
    /// </summary>
    private static readonly char[] InvalidPathChars;

    static PathSanitizer()
    {
        // set up the two arrays -- sorted once for speed.
        var c = new List<char>();
        c.AddRange(Path.GetInvalidFileNameChars());

        // Some Roadie instances run in Linux to Windows SMB clients via Samba this helps with Windows clients and invalid characters in Windows
        var badWindowsFileAndDirectoryCharacters = new List<char> { '\\', '"', '/', ':', '*', '$', '?', '\'', '<', '>', '|', '*' };
        foreach (var badWindowsFileCharacter in badWindowsFileAndDirectoryCharacters)
        {
            if (!c.Contains(badWindowsFileCharacter))
            {
                c.Add(badWindowsFileCharacter);
            }
        }

        InvalidFilenameChars = c.ToArray();

        var f = new List<char>();
        f.AddRange(Path.GetInvalidPathChars());
        foreach (var badWindowsFileCharacter in badWindowsFileAndDirectoryCharacters)
        {
            if (!f.Contains(badWindowsFileCharacter))
            {
                f.Add(badWindowsFileCharacter);
            }
        }

        InvalidFilenameChars = c.ToArray();
        InvalidPathChars = f.ToArray();

        Array.Sort(InvalidFilenameChars);
        Array.Sort(InvalidPathChars);
    }

    /// <summary>
    ///     Cleans a filename of invalid characters
    /// </summary>
    /// <param name="input">the string to clean</param>
    /// <param name="errorChar">the character which replaces bad characters</param>
    /// <returns></returns>
    public static string? SanitizeFilename(string? input, char errorChar)
    {
        return Sanitize(input, InvalidFilenameChars, errorChar);
    }

    /// <summary>
    ///     Cleans a path of invalid characters
    /// </summary>
    /// <param name="input">the string to clean</param>
    /// <param name="errorChar">the character which replaces bad characters</param>
    /// <returns></returns>
    public static string? SanitizePath(string? input, char errorChar)
    {
        return Sanitize(input, InvalidPathChars, errorChar);
    }

    /// <summary>
    ///     Cleans a string of invalid characters.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="invalidChars"></param>
    /// <param name="errorChar"></param>
    /// <returns></returns>
    private static string? Sanitize(string? input, char[] invalidChars, char errorChar)
    {
        // null always sanitizes to null
        if (input == null)
        {
            return null;
        }

        var result = new StringBuilder();
        foreach (var characterToTest in input)
        {
            // we binary search for the character in the invalid set. This should be lightning fast.
            // the character was not found in invalid, so it is valid.
            // we found the character in the array of
            result.Append(Array.BinarySearch(invalidChars, characterToTest) >= 0 ? errorChar : characterToTest);
        }

        // we're done.
        return result.ToString();
    }
}
