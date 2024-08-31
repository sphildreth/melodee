using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Track;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Directory;

/// <summary>
/// Processes Simple Verification Files (SFV) and gets files (tracks) and files CRC for release.
/// </summary>
public sealed class SimpleFileVerification(IEnumerable<ITrackPlugin> trackPlugins, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    public const string HandlesExtension = "SFV";
    
    private readonly IEnumerable<ITrackPlugin> _trackPlugins = trackPlugins;
    public override string Id => "6C253D42-F176-4A58-A895-C54BEB1F8A5C";

    public override string DisplayName => nameof(SimpleFileVerification);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 1;
    
    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }      

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var sfvFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

        if (sfvFiles.Length == 0)
        {
            return new OperationResult<int>("Skipping validation. No SFV files found.")
            {
                Type = OperationResponseType.NotFound,
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

        var trackPlugin = _trackPlugins.First();
        foreach (var sfvFile in sfvFiles)
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, sfvFile.Name))
            {
                var models = await GetModelsFromSfvFile(fileSystemDirectoryInfo, sfvFile.FullName);

                Log.Debug("\u2502 Found [{Tracks}] valid tracks for Sfv file", models.Count(x => x.IsValid));
                
                var tracks = new List<Common.Models.Track>();
                await Parallel.ForEachAsync(models.Where(x => x.IsValid), cancellationToken, async (model, tt) =>
                {
                    var trackResult = await trackPlugin.ProcessFileAsync(fileSystemDirectoryInfo, model.FileSystemFileInfo, tt);
                    if (trackResult.IsSuccess)
                    {
                        tracks.Add(trackResult.Data);
                    }
                    else
                    {
                        Log.Warning("Unable to get track details for Sfv Model [{SfvModel}]", model);
                    }
                });

                if (tracks.Count == 0)
                {
                    return new OperationResult<int>($"Unable to find tracks for directory [{fileSystemDirectoryInfo}]")
                    {
                        Type = OperationResponseType.ValidationFailure,
                        Data = -1
                    };
                }

                var firstTrack = tracks.OrderBy(x => x.SortOrder).First(x => x.Tags != null);
                var trackTotal = firstTrack.TrackTotalNumber();
                if (trackTotal < 1)
                {
                    if (models.Length == tracks.Count)
                    {
                        trackTotal = models.Length;
                    }
                }

                var newReleaseTags = new List<MetaTag<object?>>
                {
                    new() { Identifier = MetaTagIdentifier.Album, Value = firstTrack.ReleaseTitle(), SortOrder = 1 },
                    new() { Identifier = MetaTagIdentifier.AlbumArtist, Value = firstTrack.ReleaseArtist(), SortOrder = 2 },
                    new() { Identifier = MetaTagIdentifier.DiscNumber, Value = firstTrack.MediaNumber(), SortOrder = 4 },
                    new() { Identifier = MetaTagIdentifier.OrigReleaseYear, Value = firstTrack.ReleaseYear(), SortOrder = 100 },
                    new() { Identifier = MetaTagIdentifier.TrackTotal, Value = trackTotal, SortOrder = 101 }
                };
                var genres = tracks
                    .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                    .Where(x => x.Identifier == MetaTagIdentifier.Genre)
                    .ToArray();
                if (genres.Length != 0)
                {
                    newReleaseTags.AddRange(genres
                        .GroupBy(x => x.Value)
                        .Select((genre, i) => new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Genre,
                            Value = genre.Key,
                            SortOrder = 5 + i
                        }));
                }

                var sfvRelease = new Release
                {
                    Files = new[]
                    {
                        new ReleaseFile
                        {
                            ReleaseFileType = ReleaseFileType.MetaData,
                            ProcessedByPlugin = DisplayName,
                            FileSystemFileInfo = sfvFile.ToFileSystemInfo()
                        }
                    },
                    OriginalDirectory = new FileSystemDirectoryInfo
                    {
                        ParentId = parentDirectory?.UniqueId ?? 0,
                        Path = fileSystemDirectoryInfo.Path,
                        Name = fileSystemDirectoryInfo.Name,
                        TotalItemsFound = tracks.Count,
                        MusicFilesFound = tracks.Count,
                        MusicMetaDataFilesFound = 1
                    },
                    Images = tracks.Where(x => x.Images != null).SelectMany(x => x.Images!).DistinctBy(x => x.CrcHash).ToArray(),
                    Tags = newReleaseTags,
                    Tracks = tracks.OrderBy(x => x.SortOrder).ToArray(),
                    ViaPlugins = new[] { trackPlugin.DisplayName, DisplayName }
                };
                if (sfvRelease.IsValid())
                {
                    var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, sfvRelease.ToMelodeeJsonName());
                    if (File.Exists(stagingReleaseDataName))
                    {
                        if (Configuration.PluginProcessOptions.DoOverrideExistingMelodeeDataFiles)
                        {
                            File.Delete(stagingReleaseDataName);
                        }
                        else
                        {
                            var existingRelease = System.Text.Json.JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(stagingReleaseDataName, cancellationToken));
                            if (existingRelease != null)
                            {
                                sfvRelease = sfvRelease.Merge(existingRelease);
                            }
                        }
                    }

                    var serialized = System.Text.Json.JsonSerializer.Serialize(sfvRelease);
                    await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                    if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        sfvFile.Delete();
                        Log.Information("Deleted SFV File [{FileName}]", sfvFile.Name);
                    }
                    Log.Debug("[{Plugin}] created [{StagingReleaseDataName}]", DisplayName, sfvRelease.ToMelodeeJsonName());
                    processedFiles++;
                }
                else
                {
                    Log.Warning($"Did not serialize invalid release [{sfvRelease}].", sfvRelease);
                }
            }
        }

        StopProcessing = processedFiles > 0;
        return new OperationResult<int>
        {
            Data = processedFiles
        };
    }

    private static async Task<Models.SfvLine[]> GetModelsFromSfvFile(FileSystemDirectoryInfo directoryInfo, string filePath)
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
                    Log.Warning($"Skipped SFV line [{line}]", line);
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
        var directoryInfo = filePath.ToDirectoryInfo();

        try
        {
            // Sfv line is '<fileName> <CRC>'
            var lastSpace = lineFromFile.LastIndexOf(' ');
            var filename = lineFromFile.Substring(0, lastSpace);
            var crc = lineFromFile.Substring(lastSpace + 1);
            var fileSystemInfoFile = new FileSystemFileInfo
            {
                Name = filename,
                Size = 0
            };

            string? dirName = null;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var fi = new FileInfo(filePath);
                if (fi.Directory?.Exists ?? false)
                {
                    dirName = fi.DirectoryName!;
                }

                fileSystemInfoFile = new FileInfo(Path.Combine(dirName ?? string.Empty, filename)).ToFileSystemInfo();
            }
            var isCrcHashAccurate = IsCrCHashAccurate(fileSystemInfoFile.FullName(directoryInfo), crc);
            return new Models.SfvLine
            {
                IsValid = !string.IsNullOrWhiteSpace(filePath) && isCrcHashAccurate,
                CrcHash = crc,
                FileSystemFileInfo = fileSystemInfoFile,
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile);
        }

        return null;
    }

    private static bool IsCrCHashAccurate(string filename, string crcHash)
    {
        if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(crcHash))
        {
            return false;
        }
        try
        {
            var fi = new FileInfo(filename);
            if (fi.Exists)
            {
                var calculated = CRC32.Calculate(fi);
                var doesMatch = string.Equals(calculated, crcHash, StringComparison.OrdinalIgnoreCase);

                Trace.WriteLine($"IsCrCHashAccurate File [{filename}] DoesMatch [{doesMatch}] Expected [{crcHash}] Calculated [{calculated}]", "Information");

                return doesMatch;
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
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
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile);
        }

        return false;
    }
}
