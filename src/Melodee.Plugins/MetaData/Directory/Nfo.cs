using System.Diagnostics;
using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Track;

namespace Melodee.Plugins.MetaData.Directory;

public sealed partial class Nfo(IEnumerable<ITrackPlugin> trackPlugins, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    private readonly IEnumerable<ITrackPlugin> _trackPlugins = trackPlugins;

    public override string Id => "35A33042-6E57-431C-AF94-F7F803F811C4";

    public override string DisplayName => nameof(Nfo);

    public override bool IsEnabled { get; set; } = false;

    public override int SortOrder { get; } = 3;

    public async Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var cueFiles = fileSystemDirectoryInfo.FileInfosForExtension("cue").ToArray();

        if (cueFiles.Length == 0)
        {
            return new OperationResult<bool>("Skipping CUE. No CUE files found.")
            {
                Data = false
            };
        }

        var processedNfoFiles = 0;

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
            
            // var models = await GetModelsFromSfvFile(sfvFile.FullName);
            //
            // var tracks = new List<Common.Models.Track>();
            // await Parallel.ForEachAsync(models, cancellationToken, async (model, tt) =>
            // {
            //     var trackResult = await trackPlugin.ProcessFileAsync(model.FileSystemFileInfo, tt);
            //     if (trackResult.IsSuccess)
            //     {
            //         tracks.Add(trackResult.Data);
            //     }
            // });
            //
            // var firstTrack = tracks.OrderBy(x => x.SortOrder).First(x => x.Tags != null);
            // var newReleaseTags = new List<MetaTag<object?>>
            // {
            //     new() { Identifier = MetaTagIdentifier.Album, Value = firstTrack.ReleaseTitle(), SortOrder = 1},
            //     new() { Identifier = MetaTagIdentifier.Artist, Value = firstTrack.Artist(), SortOrder = 2 },
            //     new() { Identifier = MetaTagIdentifier.DiscNumber, Value = firstTrack.MediaNumber(), SortOrder = 3 },
            //     new() { Identifier = MetaTagIdentifier.OrigReleaseYear, Value = firstTrack.ReleaseYear(), SortOrder = 100 },
            //     new() { Identifier = MetaTagIdentifier.TrackTotal, Value = firstTrack.TrackTotalNumber(), SortOrder = 101 }
            // };
            // var genres = tracks
            //     .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
            //     .Where(x => x.Identifier == MetaTagIdentifier.Genre)
            //     .ToArray();
            // if (genres.Length != 0)
            // {
            //     newReleaseTags.AddRange(genres
            //         .GroupBy(x => x.Value)
            //         .Select((genre, i) => new MetaTag<object?>
            //         {
            //             Identifier = MetaTagIdentifier.Genre,
            //             Value = genre.Key,
            //             SortOrder = 5 + i
            //         }));
            // }
            //
            // var sfvRelease = new Release
            // {
            //     Directory = new FileSystemDirectoryInfo
            //     {
            //         ParentId = parentDirectory?.UniqueId ?? 0,
            //         Path = fileSystemDirectoryInfo.Path,
            //         Name = fileSystemDirectoryInfo.Name,
            //         TotalItemsFound = tracks.Count,
            //         MusicFilesFound = tracks.Count,
            //         MusicMetaDataFilesFound = 1
            //     },
            //     Tags = newReleaseTags,
            //     Tracks = tracks.OrderBy(x => x.SortOrder).ToArray(),
            //     ViaPlugins = new[] { trackPlugin.DisplayName, DisplayName }
            // };
            // if (sfvRelease.IsValid())
            // {
            //     var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, sfvRelease.ToMelodeeJsonName());
            //     if (File.Exists(stagingReleaseDataName))
            //     {
            //         var existingRelease = System.Text.Json.JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(stagingReleaseDataName, cancellationToken));
            //         if (existingRelease != null)
            //         {
            //             sfvRelease = sfvRelease.Merge(existingRelease);
            //         }
            //     }                
            //     var serialized = System.Text.Json.JsonSerializer.Serialize(sfvRelease);
            //     await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
            //     if (Configuration.PluginProcessOptions.DoDeleteOriginal)
            //     {
            //         sfvFile.Delete();
            //         Log.Information("Deleted SFV File [{FileName}]", sfvFile.Name);
             //    }
                processedNfoFiles++;
         //   }
         //   else
         //   {
         //       Trace.WriteLine($"Did not serialize invalid release [{sfvRelease }].", "Warning");
          //  }
        }

        return new OperationResult<bool>
        {
            Data = processedNfoFiles > 0
        };
    }

    private static (KeyValuePair<string, object?>, string?) ParseLine(string line, char splitChar)
    {
        string key = string.Empty;
        object? result = null;
        string? rawValue = null;

        var l = line.Nullify();
        if (!string.IsNullOrWhiteSpace(l))
        {
            try
            {
                var parts = l.Split(splitChar);
                if (parts.Length > 1)
                {
                    key = parts[0].ToAlphanumericName(false, false).ToTitleCase(false).Nullify() ?? string.Empty;
                    result = parts[1].ToAlphanumericName(false, false).ToTitleCase(false).Nullify();
                    rawValue = parts[1].Nullify();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error attempting to parse [{line}] Error [{e}]", "Error");
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
    }

    private static bool IsLineForReleaseDate(string line) => IsLineForMatches(new[] { "retaildate", "reldate" }, line);
    
    private static bool IsLineForReleaseTitle(string line) => IsLineForMatches(new[] { "title" }, line);
    
    private static bool IsLineForTrackTotal(string line) => IsLineForMatches(new[] { "tracks" }, line);
    
    private static bool IsLineForLength(string line) => IsLineForMatches(new[] { "length", "runtime" }, line);
    
    private static bool IsLineForPublisher(string line) => IsLineForMatches(new[] { "label", "publisher" }, line);

    private static bool IsLineForTrack(string line)
    {
        var l = line.ToAlphanumericName().Nullify();
        return !string.IsNullOrWhiteSpace(l) && IsLineForTrackRegex().IsMatch(line);
    }
    
    public async Task<Release> ReleaseForNfoFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        char splitChar = ':';
        
        var fileInfo = new FileInfo(filePath);
        
        var releaseTags = new List<MetaTag<object?>>();
        var tracks = new List<Common.Models.Track>();
        var messages = new List<string>();

        var metaTagsToParseFromFile = new List<MetaTagIdentifier>
        {
            MetaTagIdentifier.Artist,
            MetaTagIdentifier.Encoder,
            MetaTagIdentifier.Genre
        };
        
        foreach (var line in await File.ReadAllLinesAsync(filePath, cancellationToken))
        {
            if (IsLineForTrack(line))
            {
                var l = line.OnlyAlphaNumeric();
                var trackNumber = SafeParser.ToNumber<int>(l?.Substring(0, 2) ?? string.Empty);
                var trackDuration = l?.Substring(l.Length - 7).Trim() ?? string.Empty;
                var trackTitle = l?.Substring(3, (l.Length - trackDuration.Length) - 4).Trim() ?? string.Empty;
                tracks.Add(new Common.Models.Track
                {
                    Tags = new []
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
                        Path = fileInfo.Directory?.FullName ?? string.Empty,
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
        
        return new Release
        {
            ViaPlugins = new[] { nameof(Nfo)},
            Directory = new FileSystemDirectoryInfo
            {
                Path = fileInfo.Directory?.FullName ?? string.Empty,
                Name = fileInfo.DirectoryName ?? string.Empty
            },
            Tags = releaseTags,
            Tracks = tracks,
            Messages = messages,
            Status = ReleaseStatus.NotSet,
            SortOrder = 0
        };
    }

    [GeneratedRegex(@"[0-9]+\.+(.*)[0-9]{2}\:[0-9]{2}")]
    private static partial Regex IsLineForTrackRegex();
}