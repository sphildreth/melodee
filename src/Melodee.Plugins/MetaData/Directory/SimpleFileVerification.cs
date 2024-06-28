using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Track;
using Serilog;

namespace Melodee.Plugins.MetaData.Directory;

/// <summary>
/// Processes Simple Verification Files (SFV) and gets files (tracks) and files CRC for release.
/// </summary>
public sealed class SimpleFileVerification(IEnumerable<ITrackPlugin> trackPlugins, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    private readonly IEnumerable<ITrackPlugin> _trackPlugins = trackPlugins;
    public override string Id => "6C253D42-F176-4A58-A895-C54BEB1F8A5C";
    
    public override string DisplayName => nameof(SimpleFileVerification);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;
  
    public async Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var sfvFiles = fileSystemDirectoryInfo.FileInfosForExtension("sfv").ToArray();
        
        if (sfvFiles.Length == 0)
        {
            return new OperationResult<bool>("Skipping validation. No SFV files found.")
            {
                Data = false
            }; 
        }

        var processedSfvFiles = 0;

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
        
        var trackPlugin = _trackPlugins.First();
        foreach (var sfvFile in sfvFiles)
        {
            var models = await GetModelsFromSfvFile(sfvFile.FullName);

            var tracks = new List<Common.Models.Track>();
            await Parallel.ForEachAsync(models, cancellationToken, async (model, tt) =>
            {
                var trackResult = await trackPlugin.ProcessFileAsync(model.FileSystemFileInfo, tt);
                if (trackResult.IsSuccess)
                {
                    tracks.Add(trackResult.Data);
                }
            });

            var firstTrack = tracks.OrderBy(x => x.SortOrder).First();
            var newReleaseTags = new List<MetaTag<object?>>
            {
                new() { Identifier = MetaTagIdentifier.Album, Value = firstTrack.ReleaseTitle(), SortOrder = 1},
                new() { Identifier = MetaTagIdentifier.Artist, Value = firstTrack.Artist(), SortOrder = 2 },
                new() { Identifier = MetaTagIdentifier.DiscNumber, Value = firstTrack.MediaNumber(), SortOrder = 3 },
                new() { Identifier = MetaTagIdentifier.OrigReleaseYear, Value = firstTrack.ReleaseYear(), SortOrder = 100 },
                new() { Identifier = MetaTagIdentifier.TrackTotal, Value = firstTrack.TrackTotalNumber(), SortOrder = 101 }
            };
            var genres = tracks
                .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                .Where(x => x.Identifier == MetaTagIdentifier.Genre);
            newReleaseTags.AddRange(genres
                .GroupBy(x => x.Value)
                .Select((genre, i) => new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Genre,
                    Value = genre.Key,
                    SortOrder = 5 + i
                }));

            var sfvRelease = new Release
            {
                Directory = new FileSystemDirectoryInfo
                {
                    ParentId = parentDirectory?.UniqueId ?? 0,
                    Path = fileSystemDirectoryInfo.Path,
                    Name = fileSystemDirectoryInfo.Name,
                    TotalItemsFound = tracks.Count,
                    MusicFilesFound = tracks.Count,
                    MusicMetaDataFilesFound = 1
                },
                Tags = newReleaseTags,
                Tracks = tracks.OrderBy(x => x.SortOrder).ToArray(),
                ViaPlugins = new[] { trackPlugin.DisplayName, DisplayName }
            };
            if (sfvRelease.IsValid())
            {
                var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, sfvRelease.ToMelodeeJsonName());
                if (File.Exists(stagingReleaseDataName))
                {
                    var existingRelease = System.Text.Json.JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(stagingReleaseDataName, cancellationToken));
                    if (existingRelease != null)
                    {
                        sfvRelease = sfvRelease.Merge(existingRelease);
                    }
                }                
                var serialized = System.Text.Json.JsonSerializer.Serialize(sfvRelease);
                await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                {
                    sfvFile.Delete();
                    Log.Information("Deleted SFV File [{FileName}]", sfvFile.Name);
                }
                processedSfvFiles++;
            }
            else
            {
                Trace.WriteLine($"Did not serialize invalid release [{sfvRelease }].", "Warning");
            }
        }
        return new OperationResult<bool>
        {
            Data = processedSfvFiles > 0
        };
    }
    
    private static async Task<Models.SfvLine[]> GetModelsFromSfvFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return [];
        }
        var result = new List<Models.SfvLine>();
        try
        {
            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                if (IsLineForFileForTrack(line))
                {
                    var model = ModelFromSfvLine(filePath, line);
                    if (model != null)
                    {
                        result.Add(model);
                    }
                }
                else
                {
                    Trace.WriteLine($"Skipped SFV line [{line}]", "Information");
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "FilePath [{FilePath}]", filePath);
        }

        return result.ToArray();
    }    
    
    public static Models.SfvLine? ModelFromSfvLine(string filePath, string? lineFromFile)
    {
        if (string.IsNullOrWhiteSpace(lineFromFile))
        {
            return null;
        }

        try
        {
            var parts = lineFromFile.Split(' ');
            if (parts.Length == 2)
            {
                var trackNumberAndTitle = parts[0];
                if (string.IsNullOrWhiteSpace(trackNumberAndTitle))
                {
                    return null;
                }
                var trackNameAndTitleParts = trackNumberAndTitle.Split('-');
                var releaseArtist  = trackNameAndTitleParts.Length > 2 ? trackNameAndTitleParts[1] : null;
                string trackTitle;
                if (trackNameAndTitleParts.Length == 1)
                {
                    trackTitle = trackNameAndTitleParts[0];
                }
                else
                {
                    trackTitle = trackNameAndTitleParts.Length > 2 ? trackNameAndTitleParts[2] : trackNameAndTitleParts[1];
                }

                string? dirName = null;
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var fi = new FileInfo(filePath);
                    dirName = fi.DirectoryName!;
                }
                
                return new Models.SfvLine
                {
                    IsValid = IsCrCHashAccurate(Path.Combine(dirName ?? string.Empty, parts[0]), parts[1]),
                    CrcHash = parts[1],    
                    FileSystemFileInfo = new FileInfo(Path.Combine(dirName ?? string.Empty, parts[0])).ToFileSystemInfo(),
                    ReleaseArist = releaseArtist?.Replace("_", " ").CleanString(true),
                    TrackNumber = SafeParser.ToNumber<int>(trackNameAndTitleParts[0]),
                    TrackTitle = trackTitle.Replace("_", " ").RemoveFileExtension()!.CleanString() ?? string.Empty
                };
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile );
        }
        return null;
    }

    private static bool IsCrCHashAccurate(string filename, string crcHash)
    {
        if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(crcHash))
        {
            return false;
        }

        var fi = new FileInfo(filename);
        if (fi.Exists)
        {
            var calculated = CRC32.Calculate(fi);
            var doesMatch = string.Equals(calculated, crcHash, StringComparison.OrdinalIgnoreCase);

            Trace.WriteLine($"IsCrCHashAccurate File [{filename}] DoesMatch [{doesMatch}] Expected [{crcHash}] Calculated [{calculated}]");

            return doesMatch;
        }

        return false;
    }
  
    
    private static bool IsLineForFileForTrack(string? lineFromFile)
    {
        if (lineFromFile?.Nullify() == null)
        {
            return false;
        }

        try
        {
            if (lineFromFile.StartsWith("#") || lineFromFile.StartsWith(";"))
            {
                return false;
            }
            var lineParts = lineFromFile.Split(' ');
            var filename = string.Join(string.Empty, lineParts[..^1]);
            var ext = Path.GetExtension(filename);
            return FileHelper.IsFileMediaType(ext);
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile );
        }

        return false;
    }
}