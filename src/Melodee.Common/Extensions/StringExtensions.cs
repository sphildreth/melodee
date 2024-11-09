using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Extensions;

public static partial class StringExtensions
{
    private static readonly string YearParseRegex = "(19|20)\\d{2}";

    private static readonly string SongNumberParseRegex = @"\s*\d{2,}\s*-*\s*";

    private static readonly string VariousArtistParseRegex = @"([\[\(]*various\s*artists[\]\)]*)|([\[\(]*va[\]\)]*(\W))";

    private static readonly string SoundSongArtistParseRegex = @"(sound\s*Song[s]*)";

    private static readonly string CastRecordingSongArtistParseRegex = @"(original broadway cast|original cast*)";

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
        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        return input.Trim();
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

    public static string? CleanString(this string input, bool? doPutTheAtEnd = false, bool? doTitleCase = true)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
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
        return Regex.Replace(result.Replace("’", "'"), @"\s+", " ").Trim();
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

    public static string? RemoveSongNumberFromString(this string input)
    {
        if (input.Nullify() == null)
        {
            return null;
        }

        return Regex.IsMatch(input, SongNumberParseRegex) ? Regex.Replace(input, SongNumberParseRegex, string.Empty) : input;
    }

    public static bool IsVariousArtistValue(this string? input)
    {
        if (input.Nullify() == null)
        {
            return false;
        }

        if (string.Equals(input, "va", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Regex.IsMatch(input!, VariousArtistParseRegex, RegexOptions.IgnoreCase);
    }

    public static bool IsCastRecording(this string? input)
    {
        return input.Nullify() != null && Regex.IsMatch(input!, CastRecordingSongArtistParseRegex, RegexOptions.IgnoreCase);
    }

    public static bool IsSoundSongAristValue(this string? input)
    {
        return input.Nullify() != null && Regex.IsMatch(input!, SoundSongArtistParseRegex, RegexOptions.IgnoreCase);
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
    
    public static string ToPasswordHash(this string? plainPassword) => BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(plainPassword ?? string.Empty))).Replace("-", string.Empty);

    [GeneratedRegex("[^a-zA-Z0-9 -.:]")]
    private static partial Regex OnlyAlphaNumericRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex ReplaceMultipleSpacesRegex();
}
