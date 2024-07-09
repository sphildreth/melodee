using System.Diagnostics;
using System.Text;
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
using Microsoft.VisualBasic;
using Serilog;

namespace Melodee.Plugins.MetaData.Directory;

/// <summary>
/// If a CUE file is found then split out the MP3 into tracks. 
/// </summary>
public sealed partial class CueSheet(IEnumerable<ITrackPlugin> trackPlugins, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    private readonly IEnumerable<ITrackPlugin> _trackPlugins = trackPlugins;

    public override string Id => "3CAB0527-B13F-4C29-97AD-5541229240DD";

    public override string DisplayName => nameof(CueSheet);

    public override bool IsEnabled { get; set; } = false;

    public override int SortOrder { get; } = 0;

    public async Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var cueFiles = fileSystemDirectoryInfo.FileInfosForExtension("cue").ToArray();

        if (cueFiles.Length == 0)
        {
            return new OperationResult<bool>("Skipping CUE. No CUE files found.")
            {
                Data = true
            };
        }

        var processedCueFiles = 0;

        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        FileSystemDirectoryInfo? parentDirectory = null;
        if (dirInfo.Parent != null)
        {
            parentDirectory = new FileSystemDirectoryInfo
            {
                Path = dirInfo.Parent.FullName,
                Name = dirInfo.Parent.Name
            };
        }
        
        foreach (var cueFile in cueFiles)
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
                    var wind1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
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
                        return new OperationResult<bool>
                        {
                            Errors = new [] { ex2 },
                            Data = false
                        };
                    }
                }
                if (throwError)
                {
                    Log.Error("Error reading CUE [{CUEFileForReleaseDirectory}] [{@Error}", cueFile.FullName, ex);
                    return new OperationResult<bool>
                    {
                        Errors = new [] { ex },
                        Data = false
                    };
                }
            }
            
            if (theReader != null)
            {
                var cueModel = await ParseFileAsync(cueFile.FullName);
                if (cueModel.IsValid)
                {
                    var releaseArtist = theReader.Artist ?? cueModel.Artist() ?? throw new Exception("Invalid Artist");
                    await Parallel.ForEachAsync(cueModel.Tracks.OrderBy(x => x.SortOrder), cancellationToken, async (track, ct) =>
                    {
                        var index = cueModel.TrackIndexes.First(x => x.TrackNumber == track.TrackNumber());
                        var untilIndex = cueModel.TrackIndexes.FirstOrDefault(x => x.TrackNumber == index.TrackNumber + 1);
                        await FFMpegArguments.FromFileInput(cueModel.FileSystemFileInfo.FullName(fileSystemDirectoryInfo))
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
                            }).ProcessAsynchronously(true);

                            var readerTrack = theReader.Tracks.FirstOrDefault(x => x.TrackNumber == track.TrackNumber()) ?? throw new Exception("Unable to find Track for file");
                            var fileAtl = new ATL.Track(track.File.FullName(fileSystemDirectoryInfo))
                            {
                                Album = theReader.Title ?? cueModel.ReleaseTitle() ?? throw new Exception("Invalid Release Title"),
                                AlbumArtist = releaseArtist,
                                Comment = string.Empty,
                                DiscNumber = cueModel.MediaNumber(),
                                DiscTotal = cueModel.MediaCountValue(),
                                Title = readerTrack.Title ?? throw new Exception("Invalid Track Title"),
                                TrackNumber = readerTrack.TrackNumber,
                                TrackTotal = theReader.Tracks.Count(),
                                Genre = readerTrack.Genre,
                                Year = cueModel.ReleaseYear() ?? throw new Exception("Invalid Release year")
                            };
                            var trackArtist = readerTrack.Artist.Nullify();
                            if (trackArtist != null && !releaseArtist.DoStringsMatch(trackArtist))
                            {
                                fileAtl.Artist = trackArtist;
                            }
                            else
                            {
                                fileAtl.Artist = string.Empty;
                            }
                            if (!fileAtl.Save())
                            {
                                throw new Exception($"Unable to update metadata for file [{track.File.FullName(fileSystemDirectoryInfo)}]");
                            }                        
                        
                    });
                    
                    if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        fileSystemDirectoryInfo.DeleteAllFilesForExtension("sfv");
                        fileSystemDirectoryInfo.DeleteAllFilesForExtension("m3u");
                        fileSystemDirectoryInfo.DeleteAllFilesForExtension("nfo");
                        File.Delete(cueFile.FullName);
                        var cueFileMediaFile = new System.IO.FileInfo(Path.Combine(cueFile.DirectoryName, $"{cueFile.Name.Replace(cueFile.Extension, "")}.mp3"));
                        if (cueFileMediaFile.Exists)
                        {
                            cueFileMediaFile.Delete();
                        }

                    }
                    
                    var cueRelease = cueModel.ToRelease(fileSystemDirectoryInfo);
                    if (cueRelease.IsValid())
                    {
                        var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, cueRelease.ToMelodeeJsonName());
                        if (File.Exists(stagingReleaseDataName))
                        {
                            var existingRelease = System.Text.Json.JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(stagingReleaseDataName, cancellationToken));
                            if (existingRelease != null)
                            {
                                cueRelease = cueRelease.Merge(existingRelease);
                            }
                        }                
                        var serialized = System.Text.Json.JsonSerializer.Serialize(cueRelease);
                        await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                        if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                        {
                            cueFile.Delete();
                            Log.Information("Deleted CUE File [{FileName}]", cueFile.Name);
                        }
                        processedCueFiles++;
                    }
                    else
                    {
                        Trace.WriteLine($"Did not serialize invalid release [{cueRelease }].", "Warning");
                    }
                }
            }
        }
        StopProcessing = processedCueFiles > 0;
        return new OperationResult<bool>
        {
            Data = true
        };
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

        int? trackNumber = null;
        string? trackName = null;
        
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
                        cueSheetDataFile = (new System.IO.FileInfo(Path.Combine(fileInfo.DirectoryName, kp.Value!.Replace(" MP3", string.Empty)))).ToFileSystemInfo();
                    }
                    if (kp.Value?.Contains(" WAVE") ?? false)
                    {
                        cueSheetDataFile = (new System.IO.FileInfo(Path.Combine(fileInfo.DirectoryName, kp.Value!.Replace(" WAVE", string.Empty)))).ToFileSystemInfo();
                    }                    
                    if (kp.Value?.Contains(" BINARY") ?? false)
                    {
                        cueSheetDataFile = (new System.IO.FileInfo(Path.Combine(fileInfo.DirectoryName, kp.Value!.Replace(" BINARY", string.Empty)))).ToFileSystemInfo();
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
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int?;
                    if (trackNumber.HasValue)
                    {
                        trackIndexes.Add(ParseIndex(trackNumber.Value, lineFromFile));
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
                    if (!releaseTags.Any(x => x.Identifier == MetaTagIdentifier.Artist))
                    {
                        releaseTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = kp.Value
                        });
                    }
                    else
                    {
                        trackTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.OriginalArtist,
                            Value = kp.Value
                        });
                    }
                    break;                  
                
                case CueSheetKeyRegistry.PostGap:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int?;
                    if (trackNumber.HasValue)
                    {
                        trackGaps.Add(ParseIndex(trackNumber.Value, lineFromFile));
                    }
                    break;     
                
                case CueSheetKeyRegistry.PreGap:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int?;
                    if (trackNumber.HasValue)
                    {
                        trackGaps.Add(ParseIndex(trackNumber.Value, lineFromFile));
                    }

                    break; 

                case CueSheetKeyRegistry.Rem:
                    var v = kp.Value?.Replace("REM", "")?.Nullify();

                    MetaTagIdentifier remIdentifier = MetaTagIdentifier.Comment;
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
                    if (!releaseTags.Any(x => x.Identifier == MetaTagIdentifier.Album))
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
                            Value = kp.Value.Nullify()
                        });
                    }
                    break;
                
                case CueSheetKeyRegistry.Track:
                    trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int?;
                    trackName = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
                    if (trackNumber > 0 && !string.IsNullOrWhiteSpace(trackName as string))
                    {
                        tracks.Add(new Common.Models.Track
                        {
                            CrcHash = CRC32.Calculate(fileInfo),
                            File = new FileSystemFileInfo
                            {
                                Name = $"{trackNumber} {(trackName as string)!.ToTitleCase(false)?.ToFileNameFriendly()}.mp3",
                                Size = 0
                            },
                            Tags = trackTags.ToArray(),
                            SortOrder = trackNumber.Value
                        });
                        trackTags.Clear();
                    }
                    trackTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.TrackNumber,
                        Value = SafeParser.ToNumber<int>(kp.Value.Replace(" AUDIO", string.Empty))
                    });                    
                    break;                 
            }
        }
        
        trackNumber = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int?;
        trackName = trackTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
        if (trackNumber > 0 && !string.IsNullOrWhiteSpace(trackName as string))
        {
            tracks.Add(new Common.Models.Track
            {
                CrcHash = CRC32.Calculate(fileInfo),
                File = new FileSystemFileInfo
                {
                    Name = $"{trackNumber} {(trackName as string)!.ToTitleCase(false)?.ToFileNameFriendly()}.mp3",
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
                var dirInfo = new System.IO.DirectoryInfo(fileInfo.DirectoryName);
                var cueSheetDataFileDirectoryInfo = dirInfo.ToDirectorySystemInfo();
                
                releaseDate = dirInfo.Name.TryToGetYearFromString();
                if (releaseDate == null)
                {
                    var m3uFiles = cueSheetDataFileDirectoryInfo.FileInfosForExtension("m3u");
                    foreach (var m3uFile in m3uFiles)
                    {
                        releaseDate = m3uFile.Name.TryToGetYearFromString();
                        if (releaseDate != null)
                        {
                            break;
                        }
                    }
                }
                if (releaseDate == null)
                {
                    var sfvFiles = cueSheetDataFileDirectoryInfo.FileInfosForExtension("sfv");
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
            releaseTags.Add(new MetaTag<object?>()
            {
                Identifier = MetaTagIdentifier.OrigReleaseYear,
                Value = releaseDate
            });            
        }

        return new Models.CueSheet
        {
            FileSystemFileInfo = cueSheetDataFile!,
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
            Frames = SafeParser.ToNumber<int>(msfParts[2]),
        };
    }

    private static int ParseTrackNumberFromTrack(string lineFromFile)
    {
        var parts = lineFromFile.Split(' ');
        return SafeParser.ToNumber<int>(parts[1]);
    }
}