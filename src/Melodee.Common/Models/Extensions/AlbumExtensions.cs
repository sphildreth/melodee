using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using SerilogTimings;

namespace Melodee.Common.Models.Extensions;

public static class AlbumExtensions
{
    public static bool IsVariousArtistTypeAlbum(this Album album)
    {
        var songs = (album.Songs ?? []).ToArray();
        if (songs.Length == 0)
        {
            return false;
        }

        var genre = songs.Select(x => x.Genre().Nullify()).Distinct().ToArray();
        if (genre.Length > 0)
        {
            if (genre.Any(x => x.IsSoundSongAristValue() || x.IsVariousArtistValue() || x.IsCastRecording()))
            {
                return true;
            }
        }

        return album.Artist().IsVariousArtistValue() || album.Artist().IsSoundSongAristValue() || album.Artist().IsCastRecording();
    }

    public static bool HasSongArtists(this Album album)
    {
        var songs = (album.Songs ?? []).ToArray();
        if (songs.Length == 0)
        {
            return false;
        }

        var songArtists = songs
            .Select(x => x.SongArtist())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();
        return songArtists.Length > 0;
    }

    public static bool Delete(this Album album, string directory)
    {
        var dirInfo = new DirectoryInfo(Path.Combine(directory, album.ToDirectoryName()));
        if (dirInfo.Exists)
        {
            dirInfo.Delete(true);
            return true;
        }

        return false;
    }

    public static T? MetaTagValue<T>(this Album album, MetaTagIdentifier metaTagIdentifier)
    {
        var d = default(T?);
        if (album.Tags == null || !album.Tags.Any())
        {
            return d;
        }

        try
        {
            var vv = album.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T?));
            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
            }

            return (T?)converter.ConvertFrom(vv);
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Album [{album}] Exception [{e}]");
        }

        return d;
    }

    public static bool IsValid(this Album album, Dictionary<string, object?> configuration)
    {
        if (album.Tags?.Count() == 0)
        {
            return false;
        }

        if (album.Songs?.Count() == 0)
        {
            return false;
        }

        if (album.Songs?.Any(x => !x.IsValid(configuration)) ?? false)
        {
            return false;
        }

        var artist = album.Artist().Nullify();
        var albumTitle = album.AlbumTitle().Nullify();
        return album.UniqueId > 0 &&
               artist != null &&
               albumTitle != null &&
               album.Status is AlbumStatus.Complete or AlbumStatus.New or AlbumStatus.Ok or AlbumStatus.Reviewed &&
               album.HasValidAlbumYear(configuration); 
    }

    public static string ToMelodeeJsonName(this Album album, bool? isForAlbumDirectory = null)
    {
        if (album.UniqueId < 1)
        {
            return string.Empty;
        }
        var artist = album.Artist()?.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Artist not set on Album.");
        var albumTitle = album.AlbumTitle()?.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Title not set on Album.");
        var artistAndAlbumPart = string.Empty;
        if (!(isForAlbumDirectory ?? false))
        {
            artistAndAlbumPart = $"{artist}_{albumTitle}.";
        }

        return $"{artistAndAlbumPart}{Album.JsonFileName}";
    }

    public static string ToDirectoryName(this Album album)
    {
        if (album.UniqueId < 1)
        {
            return string.Empty;
        }        
        var artist = album.Artist()?.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Artist not set on Album.");
        var albumTitle = album.AlbumTitle()?.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Title not set on Album.");
        return $"{artist} - [{album.AlbumYear()}] {albumTitle}".ToFileNameFriendly() ?? throw new Exception($"[{album}] Unable to determine Album Directory name.");
    }

    public static AlbumArtistType ArtistType(this Album album)
    {
        var artist = album.Artist();
        if (string.IsNullOrWhiteSpace(artist))
        {
            return AlbumArtistType.NotSet;
        }

        if (artist.IsVariousArtistValue())
        {
            return AlbumArtistType.VariousArtists;
        }

        if (album.Genre() == Genres.SoundSong.ToString() || artist.IsSoundSongAristValue())
        {
            return AlbumArtistType.SoundSong;
        }

        if (album.Songs != null)
        {
            if (album.Songs.Any(x => string.Equals(x.SongArtist(), artist, StringComparison.OrdinalIgnoreCase)))
            {
                return AlbumArtistType.VariousArtists;
            }

            if (album.Songs.Any(x => !string.Equals(x.SongArtist(), artist, StringComparison.OrdinalIgnoreCase)))
            {
                return AlbumArtistType.VariousArtists;
            }
        }

        return AlbumArtistType.ArtistOrBand;
    }

    /// <summary>
    ///     Return the value set for the Artist Tag.
    /// </summary>
    public static string? Artist(this Album album)
    {
        return album.MetaTagValue<string?>(MetaTagIdentifier.AlbumArtist);
    }

    public static long ArtistUniqueId(this Album album)
    {
        return album.MetaTagValue<long?>(MetaTagIdentifier.UniqueArtistId) ?? SafeParser.Hash(album.ArtistSort() ?? album.Artist() ?? Guid.NewGuid().ToString());
    }

    public static string? ArtistSort(this Album album)
    {
        return album.MetaTagValue<string?>(MetaTagIdentifier.SortAlbumArtist);
    }

    /// <summary>
    ///     Return the value set for the Album Tag.
    /// </summary>
    public static string? AlbumTitle(this Album album)
    {
        return album.MetaTagValue<string?>(MetaTagIdentifier.Album);
    }

    /// <summary>
    ///     Return the value set for the OrigAlbumYear ?? RecordingYear ?? RecordingDateOrYear
    /// </summary>
    public static int? AlbumYear(this Album album)
    {
        return album.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumYear) ??
               album.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               album.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    public static bool HasValidAlbumYear(this Album album, Dictionary<string, object?> configuration)
    {
        var albumYear = album.AlbumYear() ?? 0;
        return albumYear > DateTime.MinValue.Year && albumYear < DateTime.MaxValue.Year &&
               albumYear >= SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMinimumAlbumYear]) && 
               albumYear <= SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumAlbumYear]);
    }

    public static int MediaCountValue(this Album album)
    {
        var discTotal = album.MetaTagValue<int?>(MetaTagIdentifier.DiscTotal);
        if (discTotal == null)
        {
            var discTotalToParse = album.MetaTagValue<string?>(MetaTagIdentifier.DiscNumberTotal);
            if (discTotalToParse != null)
            {
                var discTotalParts = discTotalToParse.Split('/');
                if (discTotalParts.Length > 1)
                {
                    discTotal = SafeParser.ToNumber<int?>(discTotalParts[1]);
                }
                else
                {
                    discTotal = SafeParser.ToNumber<int?>(discTotalToParse);
                }
            }
        }
        return discTotal ?? 0;
    }

    public static int SongTotalValue(this Album album)
    {
        var songTotalFromAlbum = album.MetaTagValue<int?>(MetaTagIdentifier.SongTotal);
        if (songTotalFromAlbum is > 0)
        {
            return songTotalFromAlbum.Value;
        }
        var songTotalFromSongNumberTotal = album.MetaTagValue<string>(MetaTagIdentifier.SongNumberTotal);
        if (songTotalFromSongNumberTotal != null)
        {
            var total = songTotalFromSongNumberTotal.Split('/');
            if (total.Length > 1)
            {
                return SafeParser.ToNumber<int?>(total[1]) ?? 0;
            }
            else
            {
                return SafeParser.ToNumber<int?>(songTotalFromSongNumberTotal) ?? 0;
            }
        }
        var songTotalFromSongs = album.Songs?.FirstOrDefault(x => x.SongTotalNumber() > 0);
        if (songTotalFromSongs != null)
        {
            return songTotalFromSongs.SongTotalNumber();
        }

        return album.Songs?.Count() ?? 0;
    }

    public static string? Genre(this Album album)
    {
        return album.MetaTagValue<string?>(MetaTagIdentifier.Genre);
    }

    public static DirectoryInfo ToOriginalDirectoryInfo(this Album album)
    {
        return new DirectoryInfo(album.OriginalDirectory.Path);
    }

    public static IEnumerable<FileInfo> FileInfosForExtension(this Album album, string extension)
    {
        var dirInfo = album.ToOriginalDirectoryInfo();
        if (!dirInfo.Exists)
        {
            return [];
        }

        return dirInfo.GetFiles($"*.{extension}", SearchOption.AllDirectories).ToArray();
    }

    /// <summary>
    ///     Is the given File related to this Album.
    /// </summary>
    public static bool IsFileForAlbum(this Album album, FileSystemInfo fileSystemInfo)
    {
        if (!fileSystemInfo.Exists)
        {
            return false;
        }

        // If there is only a single Album json file in the directory with the image, its logical that image is for that Album.
        var albumJsonFilesInDirectory = album.OriginalDirectory.FileInfosForExtension(Album.JsonFileName).Count();
        if (albumJsonFilesInDirectory == 1)
        {
            return true;
        }

        if (album.Files.Any())
        {
            var fileSystemInfoJustName = Path.GetFileNameWithoutExtension(fileSystemInfo.FullName).ToAlphanumericName();
            if (album.Files.Select(albumFile => albumFile.FileNameNoExtension().ToAlphanumericName())
                .Any(albumFileNameJustName => albumFileNameJustName.DoStringsMatch(fileSystemInfoJustName)))
            {
                return true;
            }
        }

        var normalizedName = fileSystemInfo.Name.ToAlphanumericName();
        if (string.IsNullOrEmpty(normalizedName))
        {
            return false;
        }

        try
        {
            var albumId = album.UniqueId.ToString();
            var normalizedArtistName = album.Artist()?.ToAlphanumericName() ?? string.Empty;
            var normalizedTitleName = album.AlbumTitle()?.ToAlphanumericName() ?? string.Empty;

            if (normalizedName.StartsWith(albumId) || (normalizedName.Contains(normalizedArtistName) && normalizedName.Contains(normalizedTitleName)))
            {
                return true;
            }

            if (album.Songs != null)
            {
                foreach (var song in album.Songs)
                {
                    var normalizedSongArtist = song.SongArtist()?.ToAlphanumericName() ?? string.Empty;
                    var normalizedSongName = song.Title()?.ToAlphanumericName() ?? string.Empty;
                    if ((normalizedName.Contains(normalizedArtistName) || normalizedName.Contains(normalizedSongArtist)) &&
                        normalizedName.Contains(normalizedSongName))
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Error trying to determine if file [{fileSystemInfo.FullName}] is for Album [{album}] Ex [{e.Message}]", "Error");
        }

        return false;
    }

    public static string AlbumDirectoryName(this Album album, Dictionary<string, object?> configuration)
    {
        var albumTitleValue = album.AlbumTitle();
        if (string.IsNullOrWhiteSpace(albumTitleValue))
        {
            throw new Exception("Unable to determine Album title from Album.");
        }

        var albumTitle = albumTitleValue;
        var albumPathTitle = albumTitle.ToAlphanumericName(false, false).ToDirectoryNameFriendly()?.ToTitleCase(false);
        if (string.IsNullOrEmpty(albumPathTitle))
        {
            throw new Exception($"Unable to determine Album Path for Album [{album}].");
        }

        var maxFnLength = SafeParser.ToNumber<int>(configuration[SettingRegistry.ProcessingMaximumAlbumDirectoryNameLength]) - 7;
        if (albumPathTitle.Length > maxFnLength)
        {
            albumPathTitle = albumPathTitle.Substring(0, maxFnLength);
        }

        var minimumAlbumYear = SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMinimumAlbumYear]);
        var albumDate = album.AlbumYear();
        if (albumDate.HasValue && albumDate < minimumAlbumYear)
        {
            throw new Exception($"Invalid year [{albumDate}] for Album [{album}], Minimum configured value is [{minimumAlbumYear}]");
        }

        return Path.Combine(album.ArtistDirectoryName(configuration), $"[{albumDate}] {albumPathTitle}");
    }

    public static string ArtistDirectoryName(this Album album, Dictionary<string, object?> configuration)
    {
        var artistNameToUse = album.ArtistSort() ?? album.Artist();
        if (string.IsNullOrWhiteSpace(artistNameToUse))
        {
            throw new Exception("Neither Artist or ArtistSort tag is set on Album.");
        }

        var artistDirectory = artistNameToUse.ToAlphanumericName(false, false).ToDirectoryNameFriendly()?.ToTitleCase(false);
        if (string.IsNullOrEmpty(artistDirectory))
        {
            throw new Exception($"Unable to determine artist directory for Album ArtistNameToUse [{artistNameToUse}].");
        }

        var afUpper = artistDirectory.ToUpper();
        var fnSubPart1 = afUpper.ToUpper().ToCharArray().Take(1).First();
        if (!char.IsLetterOrDigit(fnSubPart1))
        {
            fnSubPart1 = '#';
        }
        else if (char.IsNumber(fnSubPart1))
        {
            fnSubPart1 = '0';
        }

        var fnSubPart2 = afUpper.Length > 2 ? afUpper.Substring(0, 2) : afUpper;
        if (fnSubPart2.EndsWith(" "))
        {
            var pos = 1;
            while (fnSubPart2.EndsWith(" "))
            {
                pos++;
                fnSubPart2 = fnSubPart2.Substring(0, 1) + afUpper.Substring(pos, 1);
            }
        }

        var fnSubPart = Path.Combine(fnSubPart1.ToString(), fnSubPart2);
        var fnIdPart = $" [{album.ArtistUniqueId()}]";
        var maxFnLength = SafeParser.ToNumber<int>(configuration[SettingRegistry.ProcessingMaximumArtistDirectoryNameLength]) - (fnSubPart.Length + fnIdPart.Length) - 2;
        if (artistDirectory.Length > maxFnLength)
        {
            artistDirectory = artistDirectory.Substring(0, maxFnLength);
        }

        return Path.Combine(fnSubPart, $"{artistDirectory}{fnIdPart}");
    }

    public static double TotalDuration(this Album album)
    {
        return album.Songs?.Sum(x => x.Duration()) ?? 0;
    }

    public static string Duration(this Album album)
    {
        var songTotalDuration = album.Songs?.Sum(x => x.Duration()) ?? 0;
        return songTotalDuration > 0 ? new TimeInfo(SafeParser.ToNumber<decimal>(songTotalDuration)).ToFullFormattedString() : "--:--";
    }

    public static async Task<byte[]?> CoverImageBytesAsync(this Album album)
    {
        if (album.Images?.Any() == true)
        {
            var image = album.Images?.FirstOrDefault(x => x.PictureIdentifier is PictureIdentifier.Front or PictureIdentifier.SecondaryFront);
            if (image != null)
            {
                var dir = new DirectoryInfo(album.Directory?.Path ?? string.Empty);
                if (!dir.Exists)
                {
                    Trace.WriteLine($"Unable to find Directory for Album [{album}]");
                    return null;
                }

                var dirDirectoryInfo = dir.ToDirectorySystemInfo();
                if (image.FileInfo != null && image.FileInfo.Exists(dirDirectoryInfo))
                {
                    using (Operation.Time("Reading bytes for Album [{AlbumId}]", album.UniqueId))
                    {
                        return await File.ReadAllBytesAsync(image.FileInfo.FullName(dirDirectoryInfo));
                    }
                }

                Trace.WriteLine($"Unable to find Image File [{image}] for Album [{album}]");
            }
        }

        return null;
    }
}
