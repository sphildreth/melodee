using System.Text;
using System.Text.Json;
using ATL.CatalogDataReaders;
using FFMpegCore;
using FFMpegCore.Enums;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Directory.Models.Extensions;
using Melodee.Plugins.MetaData.Track;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Directory;

/// <summary>
///     If a CUE file is found then split out the MP3 into tracks.
/// </summary>
public sealed class CueSheet(IEnumerable<ITrackPlugin> trackPlugins, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    private const string HandlesExtension = "CUE";

    public readonly IEnumerable<ITrackPlugin> TrackPlugins = trackPlugins;

    public override string Id => "3CAB0527-B13F-4C29-97AD-5541229240DD";

    public override string DisplayName => nameof(CueSheet);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var cueFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

        if (cueFiles.Length == 0)
        {
            return new OperationResult<int>("Skipping CUE. No CUE files found.")
            {
                Data = -1
            };
        }

        var processedFiles = 0;
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
                        if (Configuration.PluginProcessOptions.DoDeleteOriginal)
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
                            Log.Error("Error reading CUE [{CUEFileForReleaseDirectory}] [{@Error}", cueFile.FullName, ex2);
                            return new OperationResult<int>
                            {
                                Errors = new[] { ex2 },
                                Data = 0
                            };
                        }
                    }

                    if (throwError)
                    {
                        Log.Error("Error reading CUE [{CUEFileForReleaseDirectory}] [{@Error}", cueFile.FullName, ex);
                        return new OperationResult<int>
                        {
                            Errors = new[] { ex },
                            Data = 0
                        };
                    }
                }

                if (theReader != null)
                {
                    var cueModel = await ParseFileAsync(cueFile.FullName);
                    if (cueModel is { IsValid: true })
                    {
                        var releaseArtist = theReader.Artist ?? cueModel.Artist() ?? throw new Exception("Invalid Artist");
                        await Parallel.ForEachAsync(cueModel.Tracks.OrderBy(x => x.SortOrder), cancellationToken, async (track, ct) =>
                        {
                            var index = cueModel.TrackIndexes.First(x => x.TrackNumber == track.TrackNumber());
                            var untilIndex = cueModel.TrackIndexes.FirstOrDefault(x => x.TrackNumber == index.TrackNumber + 1);
                            await FFMpegArguments.FromFileInput(cueModel.MediaFileSystemFileInfo.FullName(fileSystemDirectoryInfo))
                                .OutputToFile(track.File.FullName(fileSystemDirectoryInfo), true, options =>
                                {
                                    var seekTs = new TimeSpan(0, index.Minutes, index.Seconds);
                                    options.Seek(seekTs);
                                    if (untilIndex != null)
                                    {
                                        var untilTs = new TimeSpan(0, untilIndex.Minutes, untilIndex.Seconds);
                                        var durationTs = untilTs - seekTs;
                                        options.WithDuration(durationTs);
                                    }

                                    options.WithAudioBitrate(SafeParser.ToEnum<AudioQuality>(Configuration.MediaConvertorOptions.ConvertBitrate));
                                    options.WithAudioSamplingRate(Configuration.MediaConvertorOptions.ConvertSamplingRate);
                                    options.WithVariableBitrate(Configuration.MediaConvertorOptions.ConvertVbrLevel);
                                    options.WithAudioCodec(AudioCodec.LibMp3Lame).ForceFormat("mp3");
                                }).ProcessAsynchronously();
                        });

                        var cueRelease = cueModel.ToRelease(fileSystemDirectoryInfo);

                        if (Configuration.PluginProcessOptions.DoDeleteOriginal)
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
                        else if (Configuration.PluginProcessOptions.DoRenameConverted)
                        {
                            var movedFileName = Path.Combine(cueFile.DirectoryName!, $"{cueFile.Name}.{ Configuration.PluginProcessOptions.ConvertedExtension }");
                            cueFile.MoveTo(movedFileName);
                            Log.Debug($"\ud83d\ude9b Renamed CUE file [{cueFile.Name}] => [{ Path.GetFileName(movedFileName)}]");
                            var cueFileMediaFile = new FileInfo(Path.Combine(cueFile.DirectoryName ?? string.Empty, cueModel.MediaFileSystemFileInfo.Name));                            
                            var movedCueFileMediaFileFileName = Path.Combine(cueFileMediaFile.DirectoryName!, $"{cueFileMediaFile.Name}.{ Configuration.PluginProcessOptions.ConvertedExtension }");
                            cueFileMediaFile.MoveTo(movedCueFileMediaFileFileName);
                            Log.Debug($"\ud83d\ude9b Renamed CUE Media file [{cueFileMediaFile.Name}] => [{ Path.GetFileName(movedCueFileMediaFileFileName)}]");                           
                        }
                        fileSystemDirectoryInfo.MarkAllFilesForExtensionsSkipped(Configuration, SimpleFileVerification.HandlesExtension, M3UPlaylist.HandlesExtension, Nfo.HandlesExtension);
                        
                        var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, cueRelease.ToMelodeeJsonName());
                        if (File.Exists(stagingReleaseDataName))
                        {
                            if (Configuration.PluginProcessOptions.DoOverrideExistingMelodeeDataFiles)
                            {
                                File.Delete(stagingReleaseDataName);
                            }
                            else
                            {
                                var existingRelease = JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(stagingReleaseDataName, cancellationToken));
                                if (existingRelease != null)
                                {
                                    cueRelease = cueRelease.Merge(existingRelease);
                                }
                            }
                        }

                        var serialized = JsonSerializer.Serialize(cueRelease);
                        await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                        if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                        {
                            cueFile.Delete();
                            Log.Information("Deleted CUE File [{FileName}]", cueFile.Name);
                        }

                        Log.Debug("[{Plugin}] created [{StagingReleaseDataName}]", DisplayName, cueRelease.ToMelodeeJsonName());
                        processedFiles++;
                    }
                }
            }
        }

        StopProcessing = processedFiles > 0;
        return new OperationResult<int>
        {
            Data = processedFiles
        };
    }

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }

    public static async Task<Models.CueSheet?> ParseFileAsync(string filePath)
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

        var releaseTags = new List<MetaTag<object?>>();
        var tracks = new List<Common.Models.Track>();
        var trackTags = new List<MetaTag<object?>>();
        var trackIndexes = new List<CueIndex>();
        var trackGaps = new List<CueIndex>();

        int trackNumber;
        string? trackTitle;

        FileSystemFileInfo? cueSheetDataFile = null;

        foreach (var lineFromFile in allLinesFromFile)
        {
            var kp = SplitKeyAndValueForLine(lineFromFile);
            switch (kp.Key)
            {
                case CueSheetKeyRegistry.Catalog:
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.CatalogNumber,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.CdTextFile:
                    releaseTags.Add(new MetaTag<object?>
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
                    // PRE – Pre-emphasis enabled (audio tracks only)
                    // SCMS – Serial copy management system (not supported by all recorders)                    

                    trackTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.SubCodeFlags,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.Index:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    if (trackNumber > 0)
                    {
                        trackIndexes.Add(ParseIndex(trackNumber, lineFromFile));
                    }

                    break;

                case CueSheetKeyRegistry.Isrc:
                    trackTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.Isrc,
                        Value = kp.Value
                    });
                    break;

                case CueSheetKeyRegistry.Performer:
                    if (releaseTags.All(x => x.Identifier != MetaTagIdentifier.AlbumArtist))
                    {
                        releaseTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.AlbumArtist,
                            Value = kp.Value
                        });
                    }
                    else
                    {
                        trackTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = kp.Value
                        });
                    }

                    break;

                case CueSheetKeyRegistry.PostGap:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    if (trackNumber > 0)
                    {
                        trackGaps.Add(ParseIndex(trackNumber, lineFromFile));
                    }

                    break;

                case CueSheetKeyRegistry.PreGap:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    if (trackNumber > 0)
                    {
                        trackGaps.Add(ParseIndex(trackNumber, lineFromFile));
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
                            remIdentifier = MetaTagIdentifier.OrigReleaseYear;
                            break;

                        case CueSheetRemOptionsRegistry.TotalDiscs:
                            remIdentifier = MetaTagIdentifier.DiscNumberTotal;
                            break;

                        case CueSheetRemOptionsRegistry.DiskNumber:
                            remIdentifier = MetaTagIdentifier.DiscNumber;
                            break;
                    }

                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = remIdentifier,
                        Value = v
                    });
                    break;

                case CueSheetKeyRegistry.SongWriter:
                    if (tracks.Count == 0)
                    {
                        releaseTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Composer,
                            Value = kp.Value
                        });
                    }
                    else
                    {
                        trackTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Composer,
                            Value = kp.Value
                        });
                    }

                    break;

                case CueSheetKeyRegistry.Title:
                    if (releaseTags.All(x => x.Identifier != MetaTagIdentifier.Album))
                    {
                        releaseTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Album,
                            Value = kp.Value.Nullify()
                        });
                    }
                    else
                    {
                        trackTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Title,
                            Value = kp.Value.Nullify(),
                        });
                    }

                    break;

                case CueSheetKeyRegistry.Track:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
                    trackTitle = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
                    if (trackNumber > 0 && !string.IsNullOrWhiteSpace(trackTitle))
                    {
                        var mediaNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscNumber)?.Value as int? ?? 0;
                        trackTags.ForEach(x => x.AddProcessedBy(nameof(CueSheet)));
                        tracks.Add(new Common.Models.Track
                        {
                            CrcHash = CRC32.Calculate(fileInfo),
                            File = new FileSystemFileInfo
                            {
                                Name = TrackExtensions.TrackFileName(trackNumber, trackTitle, mediaNumber),
                                Size = 0
                            },
                            Tags = trackTags.ToArray(),
                            SortOrder = trackNumber
                        });
                        trackTags.Clear();
                    }

                    trackTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.TrackNumber,
                        Value = SafeParser.ToNumber<int>(kp.Value?.Replace(" AUDIO", string.Empty))
                    });
                    break;
            }
        }

        trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0;
        trackTitle = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
        if (trackNumber > 0 && !string.IsNullOrWhiteSpace(trackTitle))
        {
            var mediaNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscNumber)?.Value as int? ?? 0;
            tracks.Add(new Common.Models.Track
            {
                CrcHash = CRC32.Calculate(fileInfo),
                File = new FileSystemFileInfo
                {
                    Name = TrackExtensions.TrackFileName(trackNumber, trackTitle, mediaNumber),
                    Size = 0
                },
                Tags = trackTags.ToArray()
            });
            trackTags.Clear();
        }

        var releaseDate = SafeParser.ToNumber<int?>(releaseTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigReleaseYear)?.Value);
        if (releaseDate == null)
        {
            // Try to get the release date from the CUE filename
            releaseDate = fileInfo.FullName.TryToGetYearFromString();
            if (releaseDate == null)
            {
                var dirInfo = new DirectoryInfo(fileInfo.DirectoryName ?? string.Empty);
                var cueSheetDataFileDirectoryInfo = dirInfo.ToDirectorySystemInfo();

                releaseDate = dirInfo.Name.TryToGetYearFromString();
                if (releaseDate == null)
                {
                    var m3UFiles = cueSheetDataFileDirectoryInfo.FileInfosForExtension(M3UPlaylist.HandlesExtension);
                    foreach (var m3UFile in m3UFiles)
                    {
                        releaseDate = m3UFile.Name.TryToGetYearFromString();
                        if (releaseDate != null)
                        {
                            break;
                        }
                    }
                }

                if (releaseDate == null)
                {
                    var sfvFiles = cueSheetDataFileDirectoryInfo.FileInfosForExtension(SimpleFileVerification.HandlesExtension);
                    foreach (var sfvFile in sfvFiles)
                    {
                        releaseDate = sfvFile.Name.TryToGetYearFromString();
                        if (releaseDate != null)
                        {
                            break;
                        }
                    }
                }

                if (releaseDate == null)
                {
                    throw new Exception("Unable to determine Release Year for CueFile.");
                }
            }

            var releaseTagForYear = releaseTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigReleaseYear);
            if (releaseTagForYear != null)
            {
                releaseTags.Remove(releaseTagForYear);
            }

            releaseTags.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.OrigReleaseYear,
                Value = releaseDate
            });
        }

        releaseTags.ForEach(x => x.AddProcessedBy(nameof(CueSheet)));
        return new Models.CueSheet
        {
            MediaFileSystemFileInfo = cueSheetDataFile!,
            FileSystemDirectoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!).ToDirectorySystemInfo(),
            Tracks = tracks,
            TrackIndexes = trackIndexes,
            Tags = releaseTags
        };
    }

    private static KeyValuePair<string, string?> SplitKeyAndValueForLine(string lineFromFile)
    {
        var parts = lineFromFile.Nullify()?.Split(' ') ?? [];
        var k = parts[0].ToUpper();
        var v = string.Join(" ", parts[1..]).Replace("\"", "");
        return parts.Length == 1 ? new KeyValuePair<string, string?>(k, null) : new KeyValuePair<string, string?>(k, v);
    }

    private static CueIndex ParseIndex(int trackNumber, string lineFromFile)
    {
        var parts = lineFromFile.Nullify()?.Split(' ') ?? [];
        var msfParts = parts[2].Split(':');
        return new CueIndex
        {
            TrackNumber = trackNumber,
            IndexNumber = SafeParser.ToNumber<int>(parts[1]),
            Minutes = SafeParser.ToNumber<int>(msfParts[0]),
            Seconds = SafeParser.ToNumber<int>(msfParts[1]),
            Frames = SafeParser.ToNumber<int>(msfParts[2])
        };
    }
}
