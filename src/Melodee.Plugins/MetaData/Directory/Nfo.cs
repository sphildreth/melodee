using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Directory;

/// <summary>
///     Processes NFO and gets tags and tracks for release.
/// </summary>
public sealed partial class Nfo(Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    public const string HandlesExtension = "NFO";

    public override string Id => "35A33042-6E57-431C-AF94-F7F803F811C4";

    public override string DisplayName => nameof(Nfo);

    public override bool IsEnabled { get; set; } = false;

    public override int SortOrder { get; } = 3;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var nfoFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

        if (nfoFiles.Length == 0)
        {
            return new OperationResult<int>("Skipping NFO. No NFO files found.")
            {
                Data = -1
            };
        }

        var processedFiles = 0;

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

        foreach (var nfoFile in nfoFiles)
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, nfoFile.Name))
            {
                var nfoRelease = await ReleaseForNfoFileAsync(nfoFile, parentDirectory, cancellationToken);

                if (nfoRelease.IsValid(Configuration))
                {
                    var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, nfoRelease.ToMelodeeJsonName());
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
                                nfoRelease = nfoRelease.Merge(existingRelease);
                            }
                        }
                    }

                    var serialized = JsonSerializer.Serialize(nfoRelease);
                    await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                    if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        nfoFile.Delete();
                        Log.Information("Deleted NFO File [{FileName}]", nfoFile.Name);
                    }
                }
                else
                {
                    Log.Warning("Could not create release from NFO data [{nfoFile}]. Artist [{Artist}] Release Title [{ReleaseTitle}] Release Year [{ReleaseYear}]", nfoFile.Name, nfoRelease.Artist(), nfoRelease.ReleaseTitle(), nfoRelease.ReleaseYear());
                    if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        nfoFile.Delete();
                        Log.Information("Deleted NFO File [{FileName}]", nfoFile.Name);
                    }
                }

                Log.Debug("[{Plugin}] created [{StagingReleaseDataName}]", DisplayName, nfoRelease.ToMelodeeJsonName());
                processedFiles++;
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

    private static (KeyValuePair<string, object?>, string?) ParseLine(string line, char splitChar)
    {
        var key = string.Empty;
        object? result = null;
        string? rawValue = null;

        var l = line.Nullify();
        if (!string.IsNullOrWhiteSpace(l))
        {
            try
            {
                var parts = l.Replace("[:", string.Empty).Replace(":]", ":").Split(splitChar);
                if (parts.Length > 1)
                {
                    key = parts[0].ToAlphanumericName(false, false).ToTitleCase(false).Nullify() ?? string.Empty;
                    result = ReplaceMultiplePeriodsRegex().Replace(parts[1].ToAlphanumericName(false, false)?.ToTitleCase(false)?.Nullify() ?? string.Empty, string.Empty);
                    rawValue = parts[1].Nullify();
                }
            }
            catch (Exception e)
            {
                Log.Warning($"Error attempting to parse [{line}] Error [{e}]", line, e);
            }
        }

        return (new KeyValuePair<string, object?>(key, result), rawValue);
    }

    private static bool IsLineForMatches(IEnumerable<string> matches, string line)
    {
        var l = line.ToAlphanumericName().Nullify();
        if (!string.IsNullOrWhiteSpace(l))
        {
            return matches.Any(match => l.Contains(match, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    } // ReSharper disable StringLiteralTypo
    private static bool IsLineForAlbumArtist(string line)
    {
        return IsLineForMatches(new[] { "artist", "albumartist" }, line);
    }

    private static bool IsLineForReleaseDate(string line)
    {
        return IsLineForMatches(new[] { "retaildate", "reldate", "ripdate" }, line);
    }

    private static bool IsLineForReleaseTitle(string line)
    {
        return IsLineForMatches(new[] { "title" }, line);
    }

    private static bool IsLineForTrackTotal(string line)
    {
        return IsLineForMatches(new[] { "tracks" }, line);
    }

    private static bool IsLineForLength(string line)
    {
        return IsLineForMatches(new[] { "length", "runtime" }, line);
    }

    private static bool IsLineForPublisher(string line)
    {
        return IsLineForMatches(new[] { "label", "publisher" }, line);
    }

    // ReSharper enable StringLiteralTypo

    private static bool IsLineForTrack(string line)
    {
        var l = line.ToAlphanumericName().Nullify();
        return !string.IsNullOrWhiteSpace(l) && IsLineForTrackRegex().IsMatch(line);
    }

    public async Task<Release> ReleaseForNfoFileAsync(FileInfo fileInfo, FileSystemDirectoryInfo? parentDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var splitChar = ':';
        var releaseTags = new List<MetaTag<object?>>();
        var tracks = new List<Common.Models.Track>();
        var messages = new List<string>();

        var metaTagsToParseFromFile = new List<MetaTagIdentifier>
        {
            MetaTagIdentifier.AlbumArtist,
            MetaTagIdentifier.Encoder,
            MetaTagIdentifier.Genre
        };

        foreach (var line in await File.ReadAllLinesAsync(fileInfo.FullName, cancellationToken))
        {
            if (IsLineForTrack(line))
            {
                var l = line.OnlyAlphaNumeric();
                var trackNumber = SafeParser.ToNumber<int>(l?.Substring(0, 2) ?? string.Empty);
                var trackDuration = l?.Substring(l.Length - 7).Trim() ?? string.Empty;
                var trackTitle = ReplaceMultiplePeriodsRegex().Replace(l?.Substring(3, l.Length - trackDuration.Length - 4) ?? string.Empty, string.Empty).Trim();
                tracks.Add(new Common.Models.Track
                {
                    CrcHash = CRC32.Calculate(fileInfo),
                    Tags = new[]
                    {
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.TrackNumber,
                            Value = trackNumber
                        },
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Title,
                            Value = trackTitle
                        },
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Length,
                            Value = trackDuration
                        }
                    },
                    File = new FileSystemFileInfo
                    {
                        Name = string.Empty,
                        Size = 0
                    },
                    SortOrder = trackNumber
                });
                continue;
            }

            if (!string.IsNullOrEmpty(line.ToAlphanumericName().Nullify()))
            {
                var plr = ParseLine(line, splitChar);
                var kp = plr.Item1;
                var rawValue = plr.Item2;
                if (IsLineForReleaseTitle(line))
                {
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.Album,
                        Value = kp.Value
                    });
                    continue;
                }

                if (IsLineForAlbumArtist(line))
                {
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.AlbumArtist,
                        Value = kp.Value
                    });
                    continue;
                }

                if (IsLineForReleaseDate(line))
                {
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.OrigReleaseYear,
                        Value = SafeParser.ToDateTime(rawValue?.OnlyAlphaNumeric() ?? string.Empty)?.Year
                    });
                    continue;
                }

                if (IsLineForTrackTotal(line))
                {
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.TrackTotal,
                        Value = SafeParser.ToNumber<int?>(rawValue?.OnlyAlphaNumeric() ?? string.Empty)
                    });
                    continue;
                }

                if (IsLineForLength(line))
                {
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.Length,
                        Value = rawValue
                    });
                    continue;
                }

                if (IsLineForPublisher(line))
                {
                    releaseTags.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.Publisher,
                        Value = rawValue?.OnlyAlphaNumeric()
                    });
                    continue;
                }

                if (kp.Key.Nullify() != null)
                {
                    foreach (var metaTagToParse in metaTagsToParseFromFile)
                    {
                        if (kp.Key.Equals(metaTagToParse.ToString(), StringComparison.OrdinalIgnoreCase) && kp.Value != null)
                        {
                            releaseTags.Add(new MetaTag<object?>
                            {
                                Identifier = metaTagToParse,
                                Value = kp.Value
                            });
                            break;
                        }
                    }
                }
            }
        }

        var result = new Release
        {
            Files = new[]
            {
                new ReleaseFile
                {
                    ReleaseFileType = ReleaseFileType.MetaData,
                    ProcessedByPlugin = DisplayName,
                    FileSystemFileInfo = fileInfo.ToFileSystemInfo()
                }
            },
            ViaPlugins = new[] { nameof(Nfo) },
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                ParentId = parentDirectoryInfo?.UniqueId ?? 0,
                Path = fileInfo.Directory?.FullName ?? string.Empty,
                Name = fileInfo.Directory?.Name ?? string.Empty
            },
            Images = tracks.Where(x => x.Images != null).SelectMany(x => x.Images!).DistinctBy(x => x.CrcHash).ToArray(),
            Tags = releaseTags,
            Tracks = tracks,
            Messages = messages,
            Status = ReleaseStatus.NotSet,
            SortOrder = 0
        };
        result.Status = result.IsValid(Configuration) ? ReleaseStatus.Invalid : ReleaseStatus.Ok;
        return result;
    }

    [GeneratedRegex(@"[0-9]+\.+(.*)[0-9]{2}\:[0-9]{2}")]
    private static partial Regex IsLineForTrackRegex();

    [GeneratedRegex(@"\.{2,}")]
    private static partial Regex ReplaceMultiplePeriodsRegex();
}
