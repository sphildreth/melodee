using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class AlbumExtensions
{
    public static KeyValue ToKeyValue(this Album album) => new KeyValue(album.UniqueId().ToString(), album.AlbumTitle().ToNormalizedString() ?? album.AlbumTitle());
    
    public static long UniqueId(this Album album)
    {
        return Album.GenerateUniqueId(album.Artist.UniqueId(), album.MusicBrainzId, album.AlbumYear(), album.AlbumTitle());
    }

    public static bool IsStudioTypeAlbum(this Album album)
    {
        var songs = (album.Songs ?? []).ToArray();
        return songs.Length != 0 && album.Directory.IsDirectoryStudioAlbums();
    }
    
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
            if (genre.Any(x => album.Artist.IsSoundSongArist() || album.Artist.IsVariousArtist() || album.Artist.IsCastRecording()))
            {
                return true;
            }
        }

        var t = album.Tags?.ToList() ?? [];
        t.AddRange(album.Songs?.Where(x => x.Tags != null).SelectMany(x => x.Tags!) ?? []);
        if (t.Any(x => x.Identifier == MetaTagIdentifier.Composer && 
                       (ArtistExtensions.VariousArtistParseRegex.IsMatch(x.Value?.ToString() ?? string.Empty) || 
                        x.Value?.ToString().ToNormalizedString() == "VARIOUS" || x.Value?.ToString().ToNormalizedString() == "VARIOUSARTISTS")))
        {
            return true;
        }

        return album.Artist.IsVariousArtist() || album.Artist.IsSoundSongArist() || album.Artist.IsCastRecording();
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
            if (typeof(T?) == typeof(short?))
            {
                return SafeParser.ToNumber<T?>(vv.ToString());
            }

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

    public static (bool, string?) IsValid(this Album album, Dictionary<string, object?> configuration)
    {
        if (album.Tags?.Count() == 0)
        {
            return (false, "Album has no tags.");
        }

        if (album.Songs?.Count() == 0)
        {
            return (false, "Album has no songs.");
        }

        if (!album.Artist.IsValid())
        {
            return (false, "Artist is invalid.");
        }

        if (album.Songs?.Any(x => !x.IsValid(configuration)) ?? false)
        {
            return (false, "Album has no valid songs.");
        }

        var songsGroupedByMediaNumber = album.Songs?.GroupBy(x => x.MediaNumber());
        var songsGroupedByMediaNumberAndSongNumber = songsGroupedByMediaNumber?.SelectMany(x => x).GroupBy(x => x.SongNumber());
        if (songsGroupedByMediaNumberAndSongNumber?.Where(x => x.Count() > 1)?.Any() ?? false)
        {
            return (false, "Album has media and/or song numbers that are duplicated.");
        }

        var albumTitle = album.AlbumTitle().Nullify();
        if (albumTitle == null)
        {
            return (false, "Album title is invalid.");
        }

        if (album.UniqueId() < 1)
        {
            return (false, "Album unique id is invalid.");
        }

        if (!album.HasValidAlbumYear(configuration))
        {
            return (false, "Album year is invalid.");
        }

        if (album.Status is AlbumStatus.Ok or AlbumStatus.New)
        {
            return (true, null);
        }

        return (false, $"Album Status is [{album.Status.ToString()}]");
    }

    public static string ToMelodeeJsonName(this Album album, bool? isForAlbumDirectory = null)
    {
        if (album.UniqueId() < 1)
        {
            return string.Empty;
        }

        var artist = album.Artist.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Artist not set on Album.");
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
        if (album.UniqueId() < 1)
        {
            return string.Empty;
        }

        var artist = album.Artist?.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Artist not set on Album.");
        var albumTitle = album.AlbumTitle()?.ToAlphanumericName(false, false).ToTitleCase(false).Nullify()?.ToFileNameFriendly() ?? throw new Exception($"[{album}] Title not set on Album.");
        return $"{artist} - [{album.AlbumYear()}] {albumTitle}".ToFileNameFriendly() ?? throw new Exception($"[{album}] Unable to determine Album Directory name.");
    }

    public static AlbumArtistType ArtistType(this Album album)
    {
        var artistName = album.Artist.Name;
        if (album.Songs != null)
        {
            if (album.Songs.Any(x => string.Equals(x.SongArtist(), artistName, StringComparison.OrdinalIgnoreCase)))
            {
                return AlbumArtistType.VariousArtists;
            }

            if (album.Songs.Any(x => !string.Equals(x.SongArtist(), artistName, StringComparison.OrdinalIgnoreCase)))
            {
                return AlbumArtistType.VariousArtists;
            }
        }

        return album.ArtistType();
    }


    public static string? DiscSubtitle(this Album album, short discNumber)
    {
        return album.Songs?.Select(x => x.MetaTagValue<string?>(MetaTagIdentifier.DiscSetSubtitle)).FirstOrDefault(x => x.Nullify() == null);
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
        return album.MetaTagValue<int?>(MetaTagIdentifier.AlbumDate) ??
               album.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumYear) ??
               album.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               album.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    /// <summary>
    ///     Return the value set for the OrigAlbumYear ?? RecordingYear ?? RecordingDateOrYear
    /// </summary>
    public static int? OriginalAlbumYear(this Album album)
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

    public static short MediaCountValue(this Album album)
    {
        var discTotal = album.MetaTagValue<short?>(MetaTagIdentifier.DiscTotal);
        if (discTotal == null)
        {
            var discTotalToParse = album.MetaTagValue<string?>(MetaTagIdentifier.DiscNumberTotal);
            if (discTotalToParse != null)
            {
                var discTotalParts = discTotalToParse.Split('/');
                if (discTotalParts.Length > 1)
                {
                    discTotal = SafeParser.ToNumber<short?>(discTotalParts[1]);
                }
                else
                {
                    discTotal = SafeParser.ToNumber<short?>(discTotalToParse);
                }
            }
            else
            {
                discTotal = album.Songs?.FirstOrDefault(x => x.MediaTotalNumber() > 0)?.MediaTotalNumber();
            }
        }
        if (discTotal == null || (discTotal < 1 && (album.Songs?.Any() ?? false)))
        {
            discTotal = SafeParser.ToNumber<short>(album.Songs?.GroupBy(x => x.MediaNumber()).Count());
        }
        return discTotal ?? SafeParser.ToNumber<short>(album.Songs?.Any() ?? false ? 1 : 0);
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

            return SafeParser.ToNumber<int?>(songTotalFromSongNumberTotal) ?? 0;
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

    public static DirectoryInfo[] ImageDirectories(this Album album)
    {
        var result = new List<DirectoryInfo>();
        if (album.Directory.Exists())
        {
            foreach (var directory in Directory.GetDirectories(album.Directory.FullName(), "*.*", SearchOption.TopDirectoryOnly))
            {
                if (ImageHelper.ImageFilesInDirectory(directory, SearchOption.TopDirectoryOnly).Length > 0)
                {
                    result.Add(new DirectoryInfo(directory));
                }
            }
        }
        return result.ToArray();
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
            var albumId = album.UniqueId().ToString();
            var normalizedArtistName = album.Artist?.ToAlphanumericName() ?? string.Empty;
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

    /// <summary>
    ///     Return the path to the Albums folder, this does not include the Artists and Library path.
    /// </summary>
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

        return $"[{albumDate}] {albumPathTitle}/";
    }

    // public static string ArtistDirectoryName(this Album album, Dictionary<string, object?> configuration)
    // {
    //     var artistNameToUse = album.ArtistSort() ?? album.Artist;
    //     if (string.IsNullOrWhiteSpace(artistNameToUse))
    //     {
    //         throw new Exception("Neither Artist or ArtistSort tag is set on Album.");
    //     }
    //
    //     var artistDirectory = artistNameToUse.ToAlphanumericName(false, false).ToDirectoryNameFriendly()?.ToTitleCase(false);
    //     if (string.IsNullOrEmpty(artistDirectory))
    //     {
    //         throw new Exception($"Unable to determine artist directory for Album ArtistNameToUse [{artistNameToUse}].");
    //     }
    //
    //     var afUpper = artistDirectory.ToUpper();
    //     var fnSubPart1 = afUpper.ToUpper().ToCharArray().Take(1).First();
    //     if (!char.IsLetterOrDigit(fnSubPart1))
    //     {
    //         fnSubPart1 = '#';
    //     }
    //     else if (char.IsNumber(fnSubPart1))
    //     {
    //         fnSubPart1 = '0';
    //     }
    //
    //     var fnSubPart2 = afUpper.Length > 2 ? afUpper.Substring(0, 2) : afUpper;
    //     if (fnSubPart2.EndsWith(" "))
    //     {
    //         var pos = 1;
    //         while (fnSubPart2.EndsWith(" "))
    //         {
    //             pos++;
    //             fnSubPart2 = fnSubPart2.Substring(0, 1) + afUpper.Substring(pos, 1);
    //         }
    //     }
    //
    //     var fnSubPart = Path.Combine(fnSubPart1.ToString(), fnSubPart2);
    //     var fnIdPart = $" [{album.ArtistUniqueId()}]";
    //     var maxFnLength = SafeParser.ToNumber<int>(configuration[SettingRegistry.ProcessingMaximumArtistDirectoryNameLength]) - (fnSubPart.Length + fnIdPart.Length) - 2;
    //     if (artistDirectory.Length > maxFnLength)
    //     {
    //         artistDirectory = artistDirectory.Substring(0, maxFnLength);
    //     }
    //
    //     return Path.Combine(fnSubPart, $"{artistDirectory}{fnIdPart}");
    // }

    public static double TotalDuration(this Album album)
    {
        return album.Songs?.Sum(x => x.Duration()) ?? 0;
    }

    public static string Duration(this Album album)
    {
        var songTotalDuration = album.Songs?.Sum(x => x.Duration()) ?? 0;
        return songTotalDuration > 0 ? new TimeInfo(SafeParser.ToNumber<decimal>(songTotalDuration)).ToFullFormattedString() : "--:--";
    }

    /// <summary>
    ///     Return a FileInfo for the first Cover image for the album.
    /// </summary>
    /// <returns>Null if not found otherwise a FileInfo with full path to the cover image.</returns>
    public static FileInfo? CoverImage(this Album album)
    {
        if (!album.Images?.Any() ?? false)
        {
            return null;
        }

        var coverImage = album.Images?.OrderBy(x => x.SortOrder).FirstOrDefault(x => x.PictureIdentifier == PictureIdentifier.Front);
        if (coverImage?.FileInfo != null)
        {
            var result = new FileInfo(coverImage.FileInfo.FullName(album.Directory));
            return result.Exists ? result : null;
        }

        return null;
    }

    public static async Task<string?> CoverImageBase64Async(this Album album, CancellationToken cancellationToken = default)
    {
        var cover = album.CoverImage();
        if (cover != null)
        {
            var imageBytes = await File.ReadAllBytesAsync(cover.FullName, cancellationToken);
            return $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
        }

        return null;
    }

    public static async Task<byte[]?> CoverImageBytesAsync(this Album album, CancellationToken cancellationToken = default)
    {
        var coverImage = album.CoverImage();
        if (coverImage != null)
        {
            return await File.ReadAllBytesAsync(coverImage.FullName, cancellationToken);
        }

        return null;
    }

    public static AlbumQuery ToAlbumQuery(this Album album)
    {
        return new AlbumQuery
        {
            Name = $"{ album.Artist.Name } { album.AlbumTitle() }",
            MusicBrainzId = album.MusicBrainzId,
            Year = album.AlbumYear() ?? 0
        };
    }
}
