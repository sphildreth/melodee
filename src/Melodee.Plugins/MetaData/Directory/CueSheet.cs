using System.Text;
using System.Text.Json;
using ATL.CatalogDataReaders;
using FFMpegCore;
using FFMpegCore.Enums;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;

using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Validation;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Directory.Models.Extensions;
using Melodee.Plugins.MetaData.Song;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Directory;

/// <summary>
///     If a CUE file is found then split out the MP3 into Songs.
/// </summary>
public sealed class CueSheet(IEnumerable<ISongPlugin> songPlugins, IMelodeeConfiguration configuration) : AlbumMetaDataBase(configuration), IDirectoryPlugin
{
    private const string HandlesExtension = "CUE";

    public readonly IEnumerable<ISongPlugin> SongPlugins = songPlugins;

    public override string Id => "3CAB0527-B13F-4C29-97AD-5541229240DD";

    public override string DisplayName => nameof(CueSheet);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var processedFiles = 0;
        try
        {
            var cueFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

            if (cueFiles.Length == 0)
            {
                return new OperationResult<int>("Skipping CUE. No CUE files found.")
                {
                    Data = -1
                };
            }

            foreach (var cueFile in cueFiles)
            {
                using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, cueFile.Name))
                {
                    ICatalogDataReader? theReader = null;
                    try
                    {
                        theReader = CatalogDataReaderFactory.GetInstance().GetCatalogDataReader(cueFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        var throwError = true;
                        if (ex.Message.Contains("encoding name"))
                        {
                            var wind1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252) ?? Encoding.UTF8;
                            var wind1252Bytes = SafeParser.ReadFile(cueFile.FullName);
                            var utf8Bytes = Encoding.Convert(wind1252, Encoding.UTF8, wind1252Bytes);
                            var newCueFilename = Path.ChangeExtension(cueFile.FullName, "temp");
                            await File.WriteAllBytesAsync(newCueFilename, utf8Bytes, cancellationToken);
                            if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                            {
                                File.Delete(cueFile.FullName);
                                File.Move(newCueFilename, cueFile.FullName);
                            }
                            else
                            {
                                File.Copy(newCueFilename, cueFile.FullName, true);
                            }

                            try
                            {
                                theReader = CatalogDataReaderFactory.GetInstance().GetCatalogDataReader(cueFile.FullName);
                            }
                            catch (Exception ex2)
                            {
                                Log.Error("Error reading CUE [{CUEFileForAlbumDirectory}] [{@Error}", cueFile.FullName, ex2);
                                return new OperationResult<int>
                                {
                                    Errors = new[] { ex2 },
                                    Data = 0
                                };
                            }
                        }

                        if (throwError)
                        {
                            Log.Error("Error reading CUE [{CUEFileForAlbumDirectory}] [{@Error}", cueFile.FullName, ex);
                            return new OperationResult<int>
                            {
                                Errors = new[] { ex },
                                Data = 0
                            };
                        }
                    }

                    if (theReader != null)
                    {
                        var cueModel = await ParseFileAsync(cueFile.FullName, Configuration);
                        if (cueModel is { IsValid: true })
                        {
                            var withAudioBitrate = SafeParser.ToNumber<int>(Configuration[SettingRegistry.ConversionBitrate]);
                            var withAudioSamplingRate = SafeParser.ToNumber<int>(Configuration[SettingRegistry.ConversionSamplingRate]);
                            var withVariableBitrate = SafeParser.ToNumber<int>(Configuration[SettingRegistry.ConversionVbrLevel]);
                            var albumArtist = theReader.Artist ?? cueModel.Artist() ?? throw new Exception("Invalid Artist");
                            await Parallel.ForEachAsync(cueModel.Songs.OrderBy(x => x.SortOrder), cancellationToken, async (song, ct) =>
                            {
                                var index = cueModel.SongIndexes.First(x => x.SongNumber == song.SongNumber());
                                var untilIndex = cueModel.SongIndexes.FirstOrDefault(x => x.SongNumber == index.SongNumber + 1);
                                await FFMpegArguments.FromFileInput(cueModel.MediaFileSystemFileInfo.FullName(fileSystemDirectoryInfo))
                                    .OutputToFile(song.File.FullName(fileSystemDirectoryInfo), true, options =>
                                    {
                                        var seekTs = new TimeSpan(0, index.Minutes, index.Seconds);
                                        options.Seek(seekTs);
                                        if (untilIndex != null)
                                        {
                                            var untilTs = new TimeSpan(0, untilIndex.Minutes, untilIndex.Seconds);
                                            var durationTs = untilTs - seekTs;
                                            options.WithDuration(durationTs);
                                        }

                                        options.WithAudioBitrate(SafeParser.ToEnum<AudioQuality>(withAudioBitrate));
                                        options.WithAudioSamplingRate(withAudioSamplingRate);
                                        options.WithVariableBitrate(withVariableBitrate);
                                        options.WithAudioCodec(AudioCodec.LibMp3Lame).ForceFormat("mp3");
                                    }).ProcessAsynchronously();
                            });

                            var cueAlbum = cueModel.ToAlbum(fileSystemDirectoryInfo);

                            var convertedExtension = SafeParser.ToString(Configuration[SettingRegistry.ProcessingConvertedExtension]);
                            if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                            {
                                fileSystemDirectoryInfo.DeleteAllFilesForExtension(SimpleFileVerification.HandlesExtension);
                                fileSystemDirectoryInfo.DeleteAllFilesForExtension(M3UPlaylist.HandlesExtension);
                                fileSystemDirectoryInfo.DeleteAllFilesForExtension(Nfo.HandlesExtension);
                                File.Delete(cueFile.FullName);
                                var cueFileMediaFile = new FileInfo(Path.Combine(cueFile.DirectoryName ?? string.Empty, cueModel.MediaFileSystemFileInfo.Name));
                                if (cueFileMediaFile.Exists)
                                {
                                    cueFileMediaFile.Delete();
                                }
                            }
                            else if (convertedExtension.Nullify() != null)
                            {
                                var movedFileName = Path.Combine(cueFile.DirectoryName!, $"{cueFile.Name}.{ convertedExtension }");
                                cueFile.MoveTo(movedFileName);
                                Log.Debug($"\ud83d\ude9b Renamed CUE file [{cueFile.Name}] => [{ Path.GetFileName(movedFileName)}]");
                                var cueFileMediaFile = new FileInfo(Path.Combine(cueFile.DirectoryName ?? string.Empty, cueModel.MediaFileSystemFileInfo.Name));                            
                                var movedCueFileMediaFileFileName = Path.Combine(cueFileMediaFile.DirectoryName!, $"{cueFileMediaFile.Name}.{ convertedExtension }");
                                cueFileMediaFile.MoveTo(movedCueFileMediaFileFileName);
                                Log.Debug($"\ud83d\ude9b Renamed CUE Media file [{cueFileMediaFile.Name}] => [{ Path.GetFileName(movedCueFileMediaFileFileName)}]");                           
                            }
                            fileSystemDirectoryInfo.MarkAllFilesForExtensionsSkipped(Configuration, SimpleFileVerification.HandlesExtension, M3UPlaylist.HandlesExtension, Nfo.HandlesExtension);
                            var isValidCheck = cueAlbum.IsValid(Configuration);
                            if (!isValidCheck.Item1)
                            {
                                cueAlbum.ValidationMessages = cueAlbum.ValidationMessages.Append(new ValidationResultMessage
                                {
                                    Message = isValidCheck.Item2 ?? "Album failed validation.",
                                    Severity = ValidationResultMessageSeverity.Critical
                                });
                            }
                            cueAlbum.Status = isValidCheck.Item1 ? AlbumStatus.Ok : AlbumStatus.Invalid;  
                            var stagingAlbumDataName = Path.Combine(fileSystemDirectoryInfo.Path, cueAlbum.ToMelodeeJsonName());
                            if (File.Exists(stagingAlbumDataName))
                            {
                                var existingAlbum = JsonSerializer.Deserialize<Album?>(await File.ReadAllTextAsync(stagingAlbumDataName, cancellationToken));
                                if (existingAlbum != null)
                                {
                                    cueAlbum = cueAlbum.Merge(existingAlbum);
                                }
                            }

                            var serialized = JsonSerializer.Serialize(cueAlbum);
                            await File.WriteAllTextAsync(stagingAlbumDataName, serialized, cancellationToken);
                            if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                            {
                                cueFile.Delete();
                                Log.Information("Deleted CUE File [{FileName}]", cueFile.Name);
                            }

                            Log.Debug("[{Plugin}] created [{StagingAlbumDataName}]", DisplayName, cueAlbum.ToMelodeeJsonName());
                            processedFiles++;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{Name}] processing directory [{DirName}]", DisplayName, fileSystemDirectoryInfo);
            StopProcessing = true;
        }
        return new OperationResult<int>
        {
            Data = processedFiles
        };
    }

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }

    public static async Task<Models.CueSheet?> ParseFileAsync(string filePath, Dictionary<string, object?> configuration)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        var allLinesFromFile = await File.ReadAllLinesAsync(filePath);

        var albumTags = new List<MetaTag<object?>>();
        var songs = new List<Common.Models.Song>();
        var songTags = new List<MetaTag<object?>>();
        var songIndexes = new List<CueIndex>();
        var songGaps = new List<CueIndex>();

        int songNumber;
        string? songTitle;

        FileSystemFileInfo? cueSheetDataFile = null;

        foreach (var lineFromFile in allLinesFromFile)
        {
            var kp = SplitKeyAndValueForLine(lineFromFile);
            switch (kp.Key)
            {
                case CueSheetKeyRegistry.Catalog:
                    albumTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.CatalogNumber,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.CdTextFile:
                    albumTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.MusicCdIdentifier,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.File:
                    if (kp.Value?.Contains(" MP3") ?? false)
                    {
                        cueSheetDataFile = new FileInfo(Path.Combine(fileInfo.DirectoryName ?? string.Empty, kp.Value!.Replace(" MP3", string.Empty))).ToFileSystemInfo();
                    }

                    if (kp.Value?.Contains(" WAVE") ?? false)
                    {
                        cueSheetDataFile = new FileInfo(Path.Combine(fileInfo.DirectoryName ?? string.Empty, kp.Value!.Replace(" WAVE", string.Empty))).ToFileSystemInfo();
                    }

                    if (kp.Value?.Contains(" BINARY") ?? false)
                    {
                        cueSheetDataFile = new FileInfo(Path.Combine(fileInfo.DirectoryName ?? string.Empty, kp.Value!.Replace(" BINARY", string.Empty))).ToFileSystemInfo();
                    }

                    break;

                case CueSheetKeyRegistry.Flags:

                    // Only 4 Flags allowed by spec : 
                    // DCP – Digital copy permitted
                    // 4CH – Four channel audio
                    // PRE – Pre-emphasis enabled (audio Songs only)
                    // SCMS – Serial copy management system (not supported by all recorders)                    

                    songTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.SubCodeFlags,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.Index:
                    songNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    if (songNumber > 0)
                    {
                        songIndexes.Add(ParseIndex(songNumber, lineFromFile));
                    }

                    break;

                case CueSheetKeyRegistry.Isrc:
                    songTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.Isrc,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.Performer:
                    if (albumTags.All(x => x.Identifier != MetaTagIdentifier.AlbumArtist))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.AlbumArtist,
                            Value = kp.Value
                        });
                    }
                    else
                    {
                        songTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = kp.Value
                        });
                    }

                    break;

                case CueSheetKeyRegistry.PostGap:
                    songNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    if (songNumber > 0)
                    {
                        songGaps.Add(ParseIndex(songNumber, lineFromFile));
                    }

                    break;

                case CueSheetKeyRegistry.PreGap:
                    songNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    if (songNumber > 0)
                    {
                        songGaps.Add(ParseIndex(songNumber, lineFromFile));
                    }

                    break;

                case CueSheetKeyRegistry.Rem:
                    var v = kp.Value?.Replace("REM", "").Nullify();

                    var remIdentifier = MetaTagIdentifier.Comment;
                    switch (v)
                    {
                        case CueSheetRemOptionsRegistry.Genre:
                            remIdentifier = MetaTagIdentifier.Genre;
                            break;

                        case CueSheetRemOptionsRegistry.Date:
                            remIdentifier = MetaTagIdentifier.OrigAlbumYear;
                            break;

                        case CueSheetRemOptionsRegistry.TotalDiscs:
                            remIdentifier = MetaTagIdentifier.DiscNumberTotal;
                            break;

                        case CueSheetRemOptionsRegistry.DiskNumber:
                            remIdentifier = MetaTagIdentifier.DiscNumber;
                            break;
                    }

                    albumTags.Add(new MetaTag<object?>
                    {
                        Identifier = remIdentifier,
                        Value = v
                    });
                    break;

                case CueSheetKeyRegistry.SongWriter:
                    if (songs.Count == 0)
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Composer,
                            Value = kp.Value
                        });
                    }
                    else
                    {
                        songTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Composer,
                            Value = kp.Value
                        });
                    }

                    break;

                case CueSheetKeyRegistry.Title:
                    if (albumTags.All(x => x.Identifier != MetaTagIdentifier.Album))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Album,
                            Value = kp.Value.Nullify()
                        });
                    }
                    else
                    {
                        songTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Title,
                            Value = kp.Value.Nullify(),
                        });
                    }

                    break;

                case CueSheetKeyRegistry.Song:
                    songNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    songTitle = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
                    if (songNumber > 0 && !string.IsNullOrWhiteSpace(songTitle))
                    {
                        var mediaNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscNumber)?.Value as int? ?? 1;
                        //song.MediaTotalNumber()
                        var totalMediaNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscTotal)?.Value as int? ?? 1;
                        songTags.ForEach(x => x.AddProcessedBy(nameof(CueSheet)));
                        var songFileName = SongExtensions.SongFileName(
                            fileInfo,
                            SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumSongNumber]),
                            songNumber,
                            songTitle,
                            SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumMediaNumber]),
                            mediaNumber,
                            totalMediaNumber);
                        songs.Add(new Common.Models.Song
                        {
                            CrcHash = Crc32.Calculate(fileInfo),
                            File = new FileSystemFileInfo
                            {
                                Name = Path.GetFileName(songFileName),
                                Size = 0
                            },
                            Tags = songTags.ToArray(),
                            SortOrder = songNumber
                        });
                        songTags.Clear();
                    }

                    songTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.TrackNumber,
                        Value = SafeParser.ToNumber<int>(kp.Value?.Replace(" AUDIO", string.Empty))
                    });
                    break;
            }
        }

        songNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
        songTitle = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
        if (songNumber > 0 && !string.IsNullOrWhiteSpace(songTitle))
        {
            var mediaNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscNumber)?.Value as int? ?? 1;
            var totalMediaNumber = songTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscTotal)?.Value as int? ?? 1;
            var songFileName = SongExtensions.SongFileName(
                fileInfo,
                SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumSongNumber]),
                songNumber,
                songTitle,
                SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumMediaNumber]),
                mediaNumber,
                totalMediaNumber);            
            songs.Add(new Common.Models.Song
            {
                CrcHash = Crc32.Calculate(fileInfo),
                File = new FileSystemFileInfo
                {
                    Name = Path.GetFileName(songFileName),                    
                    Size = 0
                },
                Tags = songTags.ToArray()
            });
            songTags.Clear();
        }

        var albumDate = SafeParser.ToNumber<int?>(albumTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigAlbumYear)?.Value);
        if (albumDate == null)
        {
            // Try to get the Album date from the CUE filename
            albumDate = fileInfo.FullName.TryToGetYearFromString();
            if (albumDate == null)
            {
                var dirInfo = new DirectoryInfo(fileInfo.DirectoryName ?? string.Empty);
                var cueSheetDataFileDirectoryInfo = dirInfo.ToDirectorySystemInfo();

                albumDate = dirInfo.Name.TryToGetYearFromString();
                if (albumDate == null)
                {
                    var m3UFiles = cueSheetDataFileDirectoryInfo.FileInfosForExtension(M3UPlaylist.HandlesExtension);
                    foreach (var m3UFile in m3UFiles)
                    {
                        albumDate = m3UFile.Name.TryToGetYearFromString();
                        if (albumDate != null)
                        {
                            break;
                        }
                    }
                }

                if (albumDate == null)
                {
                    var sfvFiles = cueSheetDataFileDirectoryInfo.FileInfosForExtension(SimpleFileVerification.HandlesExtension);
                    foreach (var sfvFile in sfvFiles)
                    {
                        albumDate = sfvFile.Name.TryToGetYearFromString();
                        if (albumDate != null)
                        {
                            break;
                        }
                    }
                }

                if (albumDate == null)
                {
                    throw new Exception("Unable to determine Album Year for CueFile.");
                }
            }

            var albumTagForYear = albumTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigAlbumYear);
            if (albumTagForYear != null)
            {
                albumTags.Remove(albumTagForYear);
            }

            albumTags.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.OrigAlbumYear,
                Value = albumDate
            });
        }

        albumTags.ForEach(x => x.AddProcessedBy(nameof(CueSheet)));
        return new Models.CueSheet
        {
            MediaFileSystemFileInfo = cueSheetDataFile!,
            FileSystemDirectoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!).ToDirectorySystemInfo(),
            Songs = songs,
            SongIndexes = songIndexes,
            Tags = albumTags
        };
    }

    private static KeyValuePair<string, string?> SplitKeyAndValueForLine(string lineFromFile)
    {
        var parts = lineFromFile.Nullify()?.Split(' ') ?? [];
        var k = parts[0].ToUpper();
        var v = string.Join(" ", parts[1..]).Replace("\"", "");
        return parts.Length == 1 ? new KeyValuePair<string, string?>(k, null) : new KeyValuePair<string, string?>(k, v);
    }

    private static CueIndex ParseIndex(int songNumber, string lineFromFile)
    {
        var parts = lineFromFile.Nullify()?.Split(' ') ?? [];
        var msfParts = parts[2].Split(':');
        return new CueIndex
        {
            SongNumber = songNumber,
            IndexNumber = SafeParser.ToNumber<int>(parts[1]),
            Minutes = SafeParser.ToNumber<int>(msfParts[0]),
            Seconds = SafeParser.ToNumber<int>(msfParts[1]),
            Frames = SafeParser.ToNumber<int>(msfParts[2])
        };
    }
}
