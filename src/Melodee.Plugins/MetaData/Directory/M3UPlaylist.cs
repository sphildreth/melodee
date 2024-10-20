using System.Text.Json;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Validation;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Directory;

public sealed class M3UPlaylist(IEnumerable<ITrackPlugin> trackPlugins, IReleaseValidator releaseValidator, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    public const string HandlesExtension = "M3U";

    public override string Id => "800EBFEF-4A9A-4DD8-8505-056D13535D45";

    public override string DisplayName => nameof(M3UPlaylist);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 2;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var m3UFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

        if (m3UFiles.Length == 0)
        {
            return new OperationResult<int>("Skipping validation. No M3U file for Release.")
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

        var trackPlugin = trackPlugins.First();
        foreach (var m3UFile in m3UFiles)
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, m3UFile.Name))
            {
                var models = await GetModelsFromM3UFile(m3UFile.FullName);

                var tracks = new List<Common.Models.Track>();
                await Parallel.ForEachAsync(models, cancellationToken, async (model, tt) =>
                {
                    var trackResult = await trackPlugin.ProcessFileAsync(fileSystemDirectoryInfo, model.FileSystemFileInfo, tt);
                    if (trackResult.IsSuccess)
                    {
                        tracks.Add(trackResult.Data);
                    }
                });
                if (tracks.Count > 0)
                {
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

                    var m3URelease = new Release
                    {
                        Files = new[]
                        {
                            new ReleaseFile
                            {
                                ReleaseFileType = ReleaseFileType.MetaData,
                                ProcessedByPlugin = DisplayName,
                                FileSystemFileInfo = m3UFile.ToFileSystemInfo()
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
                    m3URelease.Status = releaseValidator.ValidateRelease(m3URelease)?.Data.ReleaseStatus ?? ReleaseStatus.Invalid;
                    var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, m3URelease.ToMelodeeJsonName());
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
                                m3URelease = m3URelease.Merge(existingRelease);
                            }
                        }
                    }

                    var serialized = JsonSerializer.Serialize(m3URelease);
                    await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                    if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        m3UFile.Delete();
                        Log.Information("Deleted M3U File [{FileName}]", m3UFile.Name);
                    }

                    Log.Debug("[{Plugin}] created [{StagingReleaseDataName}]", DisplayName, m3URelease.ToMelodeeJsonName());
                    processedFiles++;
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

    private static async Task<M3ULine[]> GetModelsFromM3UFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return [];
        }

        var result = new List<M3ULine>();
        try
        {
            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                var model = ModelFromM3ULine(filePath, line);
                if (model != null)
                {
                    result.Add(model);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "FilePath [{FilePath}]", filePath);
        }

        return result.ToArray();
    }

    public static M3ULine? ModelFromM3ULine(string filePath, string? lineFromFile)
    {
        if (string.IsNullOrWhiteSpace(lineFromFile))
        {
            return null;
        }

        try
        {
            var directoryInfo = filePath.ToDirectoryInfo();
            var parts = lineFromFile.Split('-');
            if (parts.Length >= 3)
            {
                var releaseArtist = parts[1];
                var trackTitle = parts[2];

                var fileSystemInfoFile = new FileSystemFileInfo
                {
                    Name = lineFromFile,
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

                    fileSystemInfoFile = new FileInfo(Path.Combine(dirName ?? string.Empty, lineFromFile)).ToFileSystemInfo();
                }

                return new M3ULine
                {
                    IsValid = !string.IsNullOrWhiteSpace(filePath) && fileSystemInfoFile.Exists(directoryInfo),
                    FileSystemFileInfo = fileSystemInfoFile,
                    ReleaseArist = releaseArtist.Replace("_", " ").CleanString(true),
                    TrackNumber = SafeParser.ToNumber<int>(parts[0]),
                    TrackTitle = trackTitle.Replace("_", " ").RemoveFileExtension()!.CleanString() ?? string.Empty
                };
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile);
        }

        return null;
    }
}
