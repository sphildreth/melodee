using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Melodee.Common.Utility;

namespace Melodee.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly string YearParseRegex = "(19|20)\\d{2}";
        
        private static readonly string TrackNumberParseRegex = @"\s*\d{2,}\s*-*\s*";
        
        private static readonly Dictionary<char, string> UnicodeAccents = new Dictionary<char, string>
        {
            {'À', "A"}, {'Á', "A"}, {'Â', "A"}, {'Ã', "A"}, {'Ä', "Ae"}, {'Å', "A"}, {'Æ', "Ae"},
            {'Ç', "C"},
            {'È', "E"}, {'É', "E"}, {'Ê', "E"}, {'Ë', "E"},
            {'Ì', "I"}, {'Í', "I"}, {'Î', "I"}, {'Ï', "I"},
            {'Ð', "Dh"}, {'Þ', "Th"},
            {'Ñ', "N"},
            {'Ò', "O"}, {'Ó', "O"}, {'Ô', "O"}, {'Õ', "O"}, {'Ö', "Oe"}, {'Ø', "Oe"},
            {'Ù', "U"}, {'Ú', "U"}, {'Û', "U"}, {'Ü', "Ue"},
            {'Ý', "Y"},
            {'ß', "ss"},
            {'à', "a"}, {'á', "a"}, {'â', "a"}, {'ã', "a"}, {'ä', "ae"}, {'å', "a"}, {'æ', "ae"},
            {'ç', "c"},
            {'è', "e"}, {'é', "e"}, {'ê', "e"}, {'ë', "e"},
            {'ì', "i"}, {'í', "i"}, {'î', "i"}, {'ï', "i"},
            {'ð', "dh"}, {'þ', "th"},
            {'ñ', "n"},
            {'ò', "o"}, {'ó', "o"}, {'ô', "o"}, {'õ', "o"}, {'ö', "oe"}, {'ø', "oe"},
            {'ù', "u"}, {'ú', "u"}, {'û', "u"}, {'ü', "ue"},
            {'ý', "y"}, {'ÿ', "y"}
        };

        public static string? Nullify(this string? input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            return input;
        }

        public static string CleanString(this string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            return Regex.Replace(input.Replace("’", "'"), @"\s+", " ").Trim();
        }

        public static bool ContainsUnicodeCharacter(this string input)
        {
            const int MaxAnsiCode = 255;
            return input.Any(c => c > MaxAnsiCode);
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
            input = new string(arr).RemoveDiacritics().RemoveUnicodeAccents().Translit();
            input = Regex.Replace(input, $"[^A-Za-z0-9{(!stripSpaces ? @"\s" : string.Empty)}{(!stripCommas ? "," : string.Empty)}]+", string.Empty);
            return input;
        }

        public static string ScrubHtml(this string value)
        {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", string.Empty).Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }

        public static string RemoveDiacritics(this string s)
        {
            var normalizedString = s.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        public static string RemoveUnicodeAccents(this string text)
        {
            return text.Aggregate(
                new StringBuilder(),
                (sb, c) =>
                {
                    string r;
                    if (UnicodeAccents.TryGetValue(c, out r))
                    {
                        return sb.Append(r);
                    }

                    return sb.Append(c);
                }).ToString();
        }

        public static string Translit(this string str)
        {
            string[] lat_up =
            {
                "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T",
                "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya"
            };
            string[] lat_low =
            {
                "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t",
                "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya"
            };
            string[] rus_up =
            {
                "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У",
                "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я"
            };
            string[] rus_low =
            {
                "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у",
                "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я"
            };
            for (var i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }

            return str;
        }

        public static bool DoStringsMatch(string string1, string string2)
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
            if(Regex.IsMatch(input, YearParseRegex, RegexOptions.RightToLeft))
            {
                return SafeParser.ToNumber<int?>(Regex.Match(input, YearParseRegex, RegexOptions.RightToLeft).Value);
            }
            return null;
        }
        
        public static int? TryToGetTrackNumberFromString(this string input)
        {
            if (input.Nullify() == null)
            {
                return null;
            }
            if(Regex.IsMatch(input, TrackNumberParseRegex))
            {
                var v = new string(Regex.Match(input, TrackNumberParseRegex).Value.Where(c => char.IsDigit(c)).ToArray());
                return SafeParser.ToNumber<int?>(v);
            }
            return null;
        }        
        
        public static string RemoveTrackNumberFromString(this string input)
        {
            if (input.Nullify() == null)
            {
                return null;
            }
            if(Regex.IsMatch(input, TrackNumberParseRegex))
            {
                return Regex.Replace(input, TrackNumberParseRegex, string.Empty);
            }
            return null;
        }            
    }
}
