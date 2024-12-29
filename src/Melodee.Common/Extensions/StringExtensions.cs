using System.Globalization;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Extensions;

public static partial class StringExtensions
{
    public const char TagsSeparator = '|';
    
    private static readonly string YearParseRegex = "(19|20)\\d{2}";

    private static readonly string SongNumberParseRegex = @"\s*\d{2,}\s*-*\s*";

    public static readonly Regex HasWithFragmentsRegex = new(@"(\s*[\(\[]*with\s+)+", RegexOptions.Compiled);
    
    public static readonly Regex HasFeatureFragmentsRegex = new(@"(\s[\(\[]*ft[\s\.]|\s*[\(\[]*feat[\s\.]|[\(\[]*(featuring))+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string RomanRegex = @"\b((?:[Xx]{1,3}|[Xx][Ll]|[Ll][Xx]{0,3})?(?:[Ii]{1,3}|[Ii][VvXx]|[Vv][Ii]{0,3})?)\b";

    private static readonly string[] Conjunctions = ["Y", "E", "I"];

    private static readonly Dictionary<char, string> UnicodeAccents = new()
    {
        { 'À', "A" }, { 'Á', "A" }, { 'Â', "A" }, { 'Ã', "A" }, { 'Ä', "Ae" }, { 'Å', "A" }, { 'Æ', "Ae" },
        { 'Ç', "C" },
        { 'È', "E" }, { 'É', "E" }, { 'Ê', "E" }, { 'Ë', "E" },
        { 'Ì', "I" }, { 'Í', "I" }, { 'Î', "I" }, { 'Ï', "I" },
        { 'Ð', "Dh" }, { 'Þ', "Th" },
        { 'Ñ', "N" },
        { 'Ò', "O" }, { 'Ó', "O" }, { 'Ô', "O" }, { 'Õ', "O" }, { 'Ö', "Oe" }, { 'Ø', "Oe" },
        { 'Ù', "U" }, { 'Ú', "U" }, { 'Û', "U" }, { 'Ü', "Ue" },
        { 'Ý', "Y" },
        { 'ß', "ss" },
        { 'à', "a" }, { 'á', "a" }, { 'â', "a" }, { 'ã', "a" }, { 'ä', "ae" }, { 'å', "a" }, { 'æ', "ae" },
        { 'ç', "c" },
        { 'è', "e" }, { 'é', "e" }, { 'ê', "e" }, { 'ë', "e" },
        { 'ì', "i" }, { 'í', "i" }, { 'î', "i" }, { 'ï', "i" },
        { 'ð', "dh" }, { 'þ', "th" },
        { 'ñ', "n" },
        { 'ò', "o" }, { 'ó', "o" }, { 'ô', "o" }, { 'õ', "o" }, { 'ö', "oe" }, { 'ø', "oe" },
        { 'ù', "u" }, { 'ú', "u" }, { 'û', "u" }, { 'ü', "ue" },
        { 'ý', "y" }, { 'ÿ', "y" }
    }; // ReSharper disable StringLiteralTypo        
    private static readonly Dictionary<string, string> MacExceptions = new()
    {
        { @"\bMacEdo", "Macedo" },
        { @"\bMacEvicius", "Macevicius" },
        { @"\bMacHado", "Machado" },
        { @"\bMacHar", "Machar" },
        { @"\bMacHin", "Machin" },
        { @"\bMacHlin", "Machlin" },
        { @"\bMacIas", "Macias" },
        { @"\bMacIulis", "Maciulis" },
        { @"\bMacKie", "Mackie" },
        { @"\bMacKle", "Mackle" },
        { @"\bMacKlin", "Macklin" },
        { @"\bMacKmin", "Mackmin" },
        { @"\bMacQuarie", "Macquarie" },
        { @"\bMacEy ", "Macey " }
    };
    // ReSharper enable StringLiteralTypo

    private static readonly Dictionary<string, string> NameCaseReplacements = new()
    {
        { "o'reilly", "O'Reilly" }
    };

    public static string? Nullify(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        var inputValue = input.Trim();
        if (string.Equals("null", inputValue, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        return inputValue;
    }

    public static string? TruncateLongString(this string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        return input[0..Math.Min(input.Length, maxLength)];
    }

    public static string ToBase64(this string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string FromBase64(this string text)
    {
        text = text.Replace('_', '/').Replace('-', '+');
        switch (text.Length % 4)
        {
            case 2:
                text += "==";
                break;
            case 3:
                text += "=";
                break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(text));
    }    

    public static string? ToTitleCase(this string input, bool doPutTheAtEnd = true)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        input = input.Replace("’", "'");
        var textInfo = new CultureInfo("en-US", false).TextInfo;
        var r = textInfo.ToTitleCase(input.Trim().ToLower());
        r = Regex.Replace(r, @"\s+", " ");
        if (doPutTheAtEnd)
        {
            if (r.StartsWith("The "))
            {
                r = $"{r.Replace("The ", string.Empty)}, The";
            }
        }

        return r.NameCase();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static string NameCase(this string nameString, bool doFixConjunction = false)
    {
        nameString = Capitalize(nameString);
        nameString = UpdateRoman(nameString);
        nameString = UpdateIrish(nameString);
        if (doFixConjunction)
        {
            nameString = FixConjunction(nameString);
        }

        nameString = Regex.Replace(nameString, @"('[A-Z])", m => m.ToString().ToLower(), RegexOptions.IgnoreCase);
        foreach (var replacement in NameCaseReplacements.Keys)
        {
            nameString = nameString.Replace(replacement, NameCaseReplacements[replacement], StringComparison.OrdinalIgnoreCase);
        }

        return nameString;
    }

    private static string FixConjunction(string nameString)
    {
        foreach (var conjunction in Conjunctions)
        {
            nameString = Regex.Replace(nameString, @"\b" + conjunction + @"\b", x => x.ToString().ToLower());
        }

        return nameString;
    }

    private static string UpdateRoman(string nameString)
    {
        var matches = Regex.Matches(nameString, RomanRegex);
        if (matches.Count > 1)
        {
            foreach (Match match in matches)
            {
                if (!string.IsNullOrEmpty(match.Value))
                {
                    nameString = Regex.Replace(nameString, match.Value, x => x.ToString().ToUpper());
                }
            }
        }

        return nameString;
    }

    private static string UpdateIrish(string nameString)
    {
        if (Regex.IsMatch(nameString, @".*?\bMac[A-Za-z^aciozj]{2,}\b") || Regex.IsMatch(nameString, @".*?\bMc"))
        {
            nameString = UpdateMac(nameString);
        }

        return nameString;
    }

    /// <summary>
    ///     Updates irish Mac & Mc.
    /// </summary>
    /// <param name="nameString"></param>
    /// <returns></returns>
    private static string UpdateMac(string nameString)
    {
        var matches = Regex.Matches(nameString, @"\b(Ma?c)([A-Za-z]+)");
        if (matches is [{ Groups.Count: 3 }])
        {
            var replacement = matches[0].Groups[1].Value;
            replacement += matches[0].Groups[2].Value.Substring(0, 1).ToUpper();
            replacement += matches[0].Groups[2].Value.Substring(1);
            nameString = nameString.Replace(matches[0].Groups[0].Value, replacement);

            // Now fix "Mac" exceptions
            foreach (var exception in MacExceptions.Keys)
            {
                nameString = Regex.Replace(nameString, exception, MacExceptions[exception]);
            }
        }

        return nameString;
    }

    private static string Capitalize(string nameString)
    {
        nameString = nameString.ToLower();
        nameString = Regex.Replace(nameString, @"\b\w", x => x.ToString().ToUpper());
        nameString = Regex.Replace(nameString, @"'\w\b", x => x.ToString().ToLower()); // Lowercase 's
        return nameString;
    }
    
    public static bool IsValueInDelimitedList(this string input, string value, char delimiter = '|')
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var p = input.Split(delimiter);
        return p.Any() && p.Any(x => x.Trim().Equals(value, StringComparison.OrdinalIgnoreCase));
    }    
    
    public static string? AddToDelimitedList(this string? input, IEnumerable<string>? values, char delimiter = '|')
    {
        var vv = values?.ToArray() ?? [];
        if (string.IsNullOrEmpty(input) && (values == null || vv.Length != 0))
        {
            return null;
        }
        if (string.IsNullOrEmpty(input))
        {
            return string.Join(delimiter.ToString(), vv);
        }
        if (values == null || vv.Length == 0)
        {
            return input;
        }
        foreach (var value in vv)
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }
            if (!input.IsValueInDelimitedList(value, delimiter))
            {
                if (!input.EndsWith(delimiter.ToString()))
                {
                    input += delimiter;
                }
                input += value;
            }
        }
        return input;
    }

    /// <summary>
    /// Return the cleaned version of the string without changing chase or manipulating the "THE" part.
    /// </summary>
    public static string? CleanStringAsIs(this string input) => CleanString(input, false, false);

    public static string? CleanString(this string? input, bool? doPutTheAtEnd = false, bool? doTitleCase = true)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        var result = input;
        result = result.Trim();
        if (doTitleCase ?? true)
        {
            result = result.ToTitleCase(doPutTheAtEnd ?? false);
        }
        if (string.IsNullOrEmpty(result))
        {
            return input;
        }
        result = NullAndUnicodeRegex().Replace(result.Replace("\u0026", "&").Replace("\0", string.Empty), string.Empty);
        return WeirdTickAndMoreThanSingleSpacesRegex().Replace(result.Replace("’", "'"), " ").Trim();
    }

    public static bool ContainsUnicodeCharacter(this string input)
    {
        const int maxAnsiCode = 255;
        return input.Any(c => c > maxAnsiCode);
    }

    public static string? OnlyAlphaNumeric(this string? input)
    {
        return string.IsNullOrEmpty(input) ? null : OnlyAlphaNumericRegex().Replace(input, " ").Nullify();
    }

    /// <summary>
    ///     If the given input is a tag delimited string, return an enumeration of tags, otherwise return null.
    /// </summary>
    /// <param name="str">Nullable string of tag delimited values</param>
    /// <returns>Enumerable collection of tags</returns>
    public static IEnumerable<string>? ToTags(this string? str)
    {
        if (SafeParser.IsNull(str))
        {
            return null;
        }
        if (!str!.Contains(TagsSeparator))
        {
            return [str];
        }
        return str.Split(TagsSeparator).Where(x => !SafeParser.IsNull(x));
    }    
    
    /// <summary>
    ///     Add tags to the given string value.
    /// </summary>
    /// <param name="str">Tag value to return tags joined by split value</param>
    /// <param name="value">Value to add to Tag result, can be Tags also joined by split value</param>
    /// <param name="tagSplit">Tag split value</param>
    /// <param name="dontLowerCase">Don't return lowercase value, leave value case as is</param>
    /// <returns>Unique and sorted Tags joined by split value</returns>
    public static string? AddTag(this string? str, string? value, char? tagSplit = TagsSeparator, bool? dontLowerCase = false)
        => AddTag(str, value?.Split(tagSplit ?? TagsSeparator), tagSplit, dontLowerCase);

    /// <summary>
    ///     Add tags to the given string value.
    /// </summary>
    /// <param name="str">Tag value to return tags joined by split value</param>
    /// <param name="values">Values to add to Tag result</param>
    /// <param name="tagSplit">Tag split value</param>
    /// <param name="dontLowerCase">Don't return lowercase value, leave value case as is</param>
    /// <param name="doReorder">Reorder values</param>
    /// <returns>Unique and sorted Tags joined by split value</returns>
    public static string? AddTag(this string? str, IEnumerable<string?>? values, char? tagSplit = TagsSeparator, bool? dontLowerCase = false, bool? doReorder = true)
    {
        var vv = values as string[] ?? values?.ToArray() ?? [];
        if (SafeParser.IsNull(str) && vv.Length == 0)
        {
            return null;
        }
        var ts = tagSplit ?? TagsSeparator;
        var value = string.Join(ts, vv.Where(x => !string.IsNullOrEmpty(x)).Select(x => x));
        var tags = (str ?? value).Nullify()?.Split(ts).Select(x => x.Nullify()).ToList() ?? [];
        if (value.Contains(ts))
        {
            tags.AddRange(value.Split(ts));
        }
        else if (!SafeParser.IsTruthy(tags.Any(x => string.Equals(x, value.Nullify(), StringComparison.OrdinalIgnoreCase))))
        {
            tags.Add(value.Nullify());
        }
        var tv = tags.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.CurrentCultureIgnoreCase);
        if (doReorder ?? true)
        {
            tv = tv.Order();
        }
        var result = string.Join(ts, tv).Nullify();
        if (dontLowerCase ?? false)
        {
            return result;
        }
        return result?.ToLower();
    }    
    
    public static string? TrimAndNullify(this string? value)
        => value?.Nullify();    
    
    public static string? ToNormalizedString(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        var v = input.CleanStringAsIs();
        if (string.IsNullOrWhiteSpace(v))
        {
            // Cheap way to handle non english, RTL, etc.
            return input.ToBase64();
        }

        v = ReplaceWithCharacter().Replace(v, string.Empty);
        return ReplaceDuplicatePipeCharacters()
            .Replace(RemoveNonAlphanumericCharacters()
                    .Replace(v.RemoveAccents()
                                 ?.ToUpperInvariant() ??
                             string.Empty,
                        string.Empty),
                "|")
            .Nullify();
    }

    public static string ToAlphanumericName(this string input, bool stripSpaces = true, bool stripCommas = true)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        input = input.ToLower()
            .Replace("$", "s")
            .Replace("%", "per");
        input = WebUtility.HtmlDecode(input);
        input = input.ScrubHtml().ToLower()
            .Replace("&", "and");
        var arr = input.ToCharArray();
        arr = Array.FindAll(arr, c => (c == ',' && !stripCommas) || (char.IsWhiteSpace(c) && !stripSpaces) || char.IsLetterOrDigit(c));
        input = new string(arr).RemoveDiacritics().RemoveUnicodeAccents().Transliteration();
        input = Regex.Replace(input, $"[^A-Za-z0-9{(!stripSpaces ? @"\s" : string.Empty)}{(!stripCommas ? "," : string.Empty)}]+", string.Empty);
        return input;
    }

    public static string? ToDirectoryNameFriendly(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        input = input.Replace("$", "s");
        return Regex.Replace(PathSanitizer.SanitizeFilename(input, ' ') ?? string.Empty, @"\s+", " ").Trim().TrimEnd('.');
    }

    private static string ScrubHtml(this string value)
    {
        var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", string.Empty).Trim();
        var step2 = Regex.Replace(step1, @"\s{2,}", " ");
        return step2;
    }

    public static string RemoveStartsWith(this string input, string remove = "")
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var index = input.IndexOf(remove, StringComparison.OrdinalIgnoreCase);
        var result = input;
        while (index == 0)
        {
            result = result.Remove(index, remove.Length).Trim();
            index = result.IndexOf(remove, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string RemoveDiacritics(this string s)
    {
        var normalizedString = s.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }

    private static string RemoveUnicodeAccents(this string text)
    {
        return text.Aggregate(
            new StringBuilder(),
            (sb, c) =>
            {
                if (UnicodeAccents.TryGetValue(c, out var r))
                {
                    return sb.Append(r);
                }

                return sb.Append(c);
            }).ToString();
    }

    private static string Transliteration(this string str)
    {
        string[] latUp =
        [
            "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T",
            "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya"
        ];
        string[] latLow =
        [
            "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t",
            "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya"
        ];
        string[] rusUp =
        [
            "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У",
            "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я"
        ];
        string[] rusLow =
        [
            "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у",
            "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я"
        ];
        for (var i = 0; i <= 32; i++)
        {
            str = str.Replace(rusUp[i], latUp[i]);
            str = str.Replace(rusLow[i], latLow[i]);
        }

        return str;
    }

    public static bool DoStringsMatch(this string? string1, string? string2)
    {
        var a1 = string1.Nullify();
        var a2 = string2.Nullify();
        if (a1 == a2)
        {
            return true;
        }

        if (a1 == null && a2 != null)
        {
            return false;
        }

        if (a1 != null && a2 == null)
        {
            return false;
        }

        return string.Equals(a1?.ToAlphanumericName(), a2?.ToAlphanumericName());
    }

    public static int? TryToGetYearFromString(this string input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }

        if (Regex.IsMatch(input, YearParseRegex, RegexOptions.RightToLeft))
        {
            return SafeParser.ToNumber<int?>(Regex.Match(input, YearParseRegex, RegexOptions.RightToLeft).Value);
        }

        return null;
    }

    public static int? TryToGetSongNumberFromString(this string input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }

        if (Regex.IsMatch(input, SongNumberParseRegex))
        {
            var v = new string(Regex.Match(input, SongNumberParseRegex).Value.Where(char.IsDigit).ToArray());
            return SafeParser.ToNumber<int?>(v);
        }

        return null;
    }
    
    public static short? TryToGetMediaNumberFromString(this string input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }
        if (FileSystemDirectoryInfoExtensions.IsDirectoryAlbumMediaDirectoryRegex.IsMatch(input))
        {
            var v = input.Where(char.IsDigit).ToArray();
            var n = SafeParser.ToNumber<short?>(new string(v));
            return n < 1 ? 1 : n;
        }
        return null;
    }    

    public static string? RemoveSongNumberFromString(this string input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }

        return Regex.IsMatch(input, SongNumberParseRegex) ? Regex.Replace(input, SongNumberParseRegex, string.Empty) : input;
    }



    public static bool HasWithFragments(this string? input)
    {
        return input.Nullify() != null && HasWithFragmentsRegex.IsMatch(input!);
    }
    
    public static bool HasFeaturingFragments(this string? input)
    {
        return input.Nullify() != null && HasFeatureFragmentsRegex.IsMatch(input!);
    }
    
    public static int FeaturingAndWithFragmentsCount(this string? input)
    {
        if (input.Nullify() == null)
        {
            return 0;
        }
        return HasWithFragmentsRegex.Matches(input!).Count + HasFeatureFragmentsRegex.Matches(input!).Count;
    }    

    public static string? RemoveFileExtension(this string? input)
    {
        return input.Nullify() == null ? null : Path.GetFileNameWithoutExtension(input!);
    }

    public static FileSystemDirectoryInfo ToDirectoryInfo(this string? input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (Path.HasExtension(input))
            {
                var fileInfo = new FileInfo(input);
                return fileInfo.Directory?.ToDirectorySystemInfo() ?? FileSystemDirectoryInfo.Blank();
            }

            return new DirectoryInfo(input).ToDirectorySystemInfo();
        }

        return new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        };
    }

    public static string? ToCleanedMultipleArtistsValue(this string? input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }

        var result = ReplaceMultipleSpacesRegex().Replace(input!.Replace("«multiple values»", string.Empty).Trim().Replace(";;", ";").Replace("; ", "/").Replace(";", "/"), string.Empty);
        if (result.StartsWith('/'))
        {
            return result[1..];
        }

        return result;
    }
    
    public static string? ToHexString(this string? input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }
        var sb = new StringBuilder();
        var bytes = Encoding.UTF8.GetBytes(input!);
        foreach (var t in bytes)
        {
            sb.Append(t.ToString("X2"));
        }
        return sb.ToString();
    }

    public static string? FromHexString(this string? input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }
        var inputVal = input?.StartsWith("enc:") ?? false ? input[4..] : input ?? string.Empty;
        if (inputVal.Nullify() == null)
        {
            return null;
        }
        var bytes = new byte[inputVal.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(inputVal.Substring(i * 2, 2), 16);
        }
        return Encoding.UTF8.GetString(bytes);
    }    
    
    private static string? RemoveAccents(this string? value)
    {
        if (SafeParser.IsNull(value))
        {
            return null;
        }
        return new string(value!.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
    }
    
    public static string? ToSafeXmlString(this string? input) => SecurityElement.Escape(input);
    
    [GeneratedRegex("[^a-zA-Z0-9 -.:]")]
    private static partial Regex OnlyAlphaNumericRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex ReplaceMultipleSpacesRegex();
    
    [GeneratedRegex("[^A-Z0-9|]")]
    private static partial Regex RemoveNonAlphanumericCharacters();

    [GeneratedRegex("(\\|{2,})")]
    public static partial Regex ReplaceDuplicatePipeCharacters();

    [GeneratedRegex("\t+|\r+|\\s+|\r+|@+")]
    public static partial Regex ReplaceWithCharacter();
    [GeneratedRegex(@"([\\]+.?[0-9]+)|([^\u0000-\u007F]+)")]
    private static partial Regex NullAndUnicodeRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex WeirdTickAndMoreThanSingleSpacesRegex();
}
