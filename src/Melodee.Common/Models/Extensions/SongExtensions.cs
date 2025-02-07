using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class SongExtensions
{
    private static readonly Regex UnwantedSongTitleTextRegex = new(@"(\s{2,}|(\s\(prod\s))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static T? MetaTagValue<T>(this Song song, MetaTagIdentifier metaTagIdentifier)
    {
        var d = default(T?);
        if (song.Tags == null || !song.Tags.Any())
        {
            return d;
        }

        try
        {
            var vv = song.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }

            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
            }

            var tType = typeof(T?);

            var converter = TypeDescriptor.GetConverter(tType);
            if (tType == typeof(short) || tType == typeof(short?))
            {
                return (T?)converter.ConvertFrom(short.Parse(vv.ToString() ?? string.Empty));
            }

            if (tType == typeof(Guid) || tType == typeof(Guid?))
            {
                var g = Guid.Parse(vv.ToString() ?? string.Empty);
                return g == Guid.Empty ? d : (T?)converter.ConvertFrom(g);
            }

            if (tType == typeof(DateTime) || tType == typeof(DateTime?))
            {
                var dt = DateTime.Parse(vv.ToString() ?? string.Empty, CultureInfo.InvariantCulture);
                return dt == DateTime.MinValue ? d : (T?)converter.ConvertFrom(dt);
            }

            return (T?)converter.ConvertFrom(vv);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }

        return d;
    }

    public static T? MediaAudioValue<T>(this Song song, MediaAudioIdentifier mediaAudioIdentifier)
    {
        var d = default(T?);
        if (song.MediaAudios == null || !song.MediaAudios.Any())
        {
            return d;
        }

        try
        {
            var vv = song.MediaAudios?.FirstOrDefault(x => x.Identifier == mediaAudioIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }

            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T?));
            return (T?)converter.ConvertFrom(vv);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }

        return d;
    }

    public static string? Comment(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Comment);
    }

    public static string? SongArtist(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Artist);
    }

    public static long? SongArtistUniqueId(this Song song)
    {
        var songArtist = song.SongArtist().Nullify();
        if (songArtist == null)
        {
            return null;
        }

        return SafeParser.Hash(songArtist);
    }

    public static long? SongArtistAlbumUniqueId(this Song song)
    {
        return SafeParser.Hash(song.AlbumArtist() ?? string.Empty, song.AlbumTitle() ?? string.Empty);
    }

    public static string? AlbumArtist(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.AlbumArtist);
    }

    public static string? AlbumTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Album);
    }


    public static int? AlbumYear(this Song song)
    {
        return (song.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                      song.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumDate) ??
                      song.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear)) ?? SafeParser.ToDateTime(song.AlbumDate())?.Year;
    }

    public static string? AlbumDate(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Date) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.AlbumDate) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.OrigAlbumDate) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    public static double? Duration(this Song song)
    {
        return song.MediaAudioValue<double?>(MediaAudioIdentifier.DurationMs);
    }

    public static string? DurationTime(this Song song)
    {
        return song.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(song.Duration()!.Value)).ToFullFormattedString() : "--:--";
    }

    public static string? DurationTimeShort(this Song song)
    {
        return song.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(song.Duration()!.Value)).ToShortFormattedString() : "--:--";
    }

    public static string? Title(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Title);
    }

    public static string? SubTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.SubTitle);
    }

    public static DateTime? AlbumDateValue(this Song song)
    {
        return song.AlbumDate().Nullify() == null ? null : DateTime.Parse(song.AlbumDate()!, CultureInfo.InvariantCulture);
    }

    public static int SongNumber(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;
    }

    public static short SongTotalNumber(this Song song)
    {
        return song.MetaTagValue<short?>(MetaTagIdentifier.SongTotal) ?? 0;
    }

    public static string? MediaSubTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.SubTitle);
    }

    public static string? Genre(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Genre);
    }

    public static int BitRate(this Song song)
    {
        return SafeParser.ToNumber<int>(song?.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value);
    }

    public static int BitDepth(this Song song)
    {
        return SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value);
    }

    public static int? ChannelCount(this Song song)
    {
        return SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value);
    }

    public static bool IsVbr(this Song song)
    {
        return SafeParser.ToNumber<bool>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.IsVbr)?.Value);
    }

    public static int SamplingRate(this Song song)
    {
        return SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value);
    }

    public static bool IsValid(this Song song, Dictionary<string, object?> configuration)
    {
        if (song.Tags?.Count() == 0)
        {
            return false;
        }

        var songNumber = song.SongNumber();
        return songNumber > 0 &&
               songNumber < SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumSongNumber]) &&
               song.Title().Nullify() != null;
    }

    public static bool TitleHasUnwantedText(this Song song)
    {
        var albumTitle = song.AlbumTitle() ?? string.Empty;
        var songTitle = song.Title();
        if (string.IsNullOrWhiteSpace(songTitle))
        {
            return true;
        }

        if (songTitle.HasFeaturingFragments())
        {
            return true;
        }

        try
        {
            if (UnwantedSongTitleTextRegex.IsMatch(songTitle))
            {
                return true;
            }

            if (songTitle.Any(char.IsDigit))
            {
                var songNumber = song.SongNumber();
                if (string.Equals(songTitle.Trim(), songNumber.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return Regex.IsMatch(songTitle, $@"^({Regex.Escape(albumTitle)}\s*.*\s*)?([0-9]*{songNumber}\s)");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"TitleHasUnwantedText For AlbumTitle [{albumTitle}] for SongTitle [{songTitle}] Error [{ex.Message}] ", "Error");
        }

        return false;
    }

    public static string ContentType(this Song song)
    {
        return MimeTypes.GetMimeType(song.File.Name);
    }

    public static string FileExtension(this Song song)
    {
        return Path.GetExtension(song.File.Name);
    }

    public static string SongFileName(FileInfo fileInfo,
        int maximumSongNumber,
        int songNumber,
        string? songTitle,
        string? extension = null)
    {
        if (songNumber < 1)
        {
            Trace.WriteLine($"File [{fileInfo}] has invalid Song number [{songNumber}]");
            songNumber = 0;
        }

        if (string.IsNullOrWhiteSpace(songTitle))
        {
            Trace.WriteLine($"File [{fileInfo}] has invalid Song title [{songTitle}]");
            songTitle = fileInfo.Name;
        }
        var songNumberPaddingLength = SafeParser.ToNumber<short>(maximumSongNumber.ToString().Length);
        var songNumberValue = songNumber.ToStringPadLeft(songNumberPaddingLength);
        var fileNameFromTitle = songTitle.ToTitleCase(false)?.ToFileNameFriendly();
        if (fileNameFromTitle != null && fileNameFromTitle.StartsWith(songNumberValue))
        {
            fileNameFromTitle = fileNameFromTitle
                .RemoveStartsWith($"{songNumberValue} -")
                .RemoveStartsWith($"{songNumberValue} ")
                .RemoveStartsWith($"{songNumberValue}.")
                .RemoveStartsWith($"{songNumberValue}-")
                .ToTitleCase(false);
        }

        return $"{songNumberValue} {fileNameFromTitle}{extension ?? fileInfo.Extension}";
    }

    public static string ToSongFileName(this Song song, FileSystemDirectoryInfo directoryInfo)
    {
        return SongFileName(
            song.File.ToFileInfo(directoryInfo),
            9999,
            song.SongNumber(),
            song.Title());
    }
}
