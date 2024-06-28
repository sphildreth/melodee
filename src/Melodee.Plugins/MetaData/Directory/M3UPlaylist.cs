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

public sealed class M3UPlaylist(IEnumerable<ITrackPlugin> trackPlugins, Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    private readonly IEnumerable<ITrackPlugin> _trackPlugins = trackPlugins;
    
    public override string Id => "800EBFEF-4A9A-4DD8-8505-056D13535D45";
    
    public override string DisplayName => nameof(M3UPlaylist);

    public override bool IsEnabled { get; set; } = false;

    public override int SortOrder { get; } = 0;

    public async Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var m3UFiles = fileSystemDirectoryInfo.FileInfosForExtension("m3u").ToArray();
        
        if (m3UFiles.Length == 0)
        {
            return new OperationResult<bool>("Skipping validation. No M3U file for Release.")
            {
                Data = false
            }; 
        }
        var processedM3ULines = 0;

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
        foreach (var m3UFile in m3UFiles)
        {
            var models = await GetModelsFromM3UFile(m3UFile.FullName);

            var tracks = new List<Common.Models.Track>();
            await Parallel.ForEachAsync(models, cancellationToken, async (model, tt) =>
            {
                var trackResult = await trackPlugin.ProcessFileAsync(model.FileSystemFileInfo, tt);
                if (trackResult.IsSuccess)
                {
                    tracks.Add(trackResult.Data);
                }
            });

            if (tracks.Count > 0)
            {
                var firstTrack = tracks.OrderBy(x => x.SortOrder).First();
                var newReleaseTags = new List<MetaTag<object?>>
                {
                    new() { Identifier = MetaTagIdentifier.Album, Value = firstTrack.ReleaseTitle(), SortOrder = 1 },
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

                var m3URelease = new Release
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
                if (m3URelease.IsValid())
                {
                    var stagingReleaseDataName = Path.Combine(fileSystemDirectoryInfo.Path, m3URelease.ToMelodeeJsonName());
                    if (File.Exists(stagingReleaseDataName))
                    {
                        var existingRelease = System.Text.Json.JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(stagingReleaseDataName, cancellationToken));
                        if (existingRelease != null)
                        {
                            m3URelease = m3URelease.Merge(existingRelease);
                        }
                    }

                    var serialized = System.Text.Json.JsonSerializer.Serialize(m3URelease);
                    await File.WriteAllTextAsync(stagingReleaseDataName, serialized, cancellationToken);
                    if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        m3UFile.Delete();
                        Log.Information("Deleted M3U File [{FileName}]", m3UFile.Name);
                    }

                    processedM3ULines++;
                }
                else
                {
                    Trace.WriteLine($"Did not serialize invalid release [{m3URelease}].", "Warning");
                }
            }
        }
        return new OperationResult<bool>
        {
            Data = processedM3ULines > 0
        };        
    }
    
    private static async Task<Models.M3ULine[]> GetModelsFromM3UFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return [];
        }
        var result = new List<Models.M3ULine>();
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

    public static Models.M3ULine? ModelFromM3ULine(string filePath, string? lineFromFile)
    {
        if (string.IsNullOrWhiteSpace(lineFromFile))
        {
            return null;
        }
        try
        {
            var parts = lineFromFile.Split('-');
            if (parts.Length >= 3)
            {
                var releaseArtist = parts[1];
                string trackTitle = parts[2];
                
                var fileSystemInfoFile = new FileSystemFileInfo
                {
                    Name = lineFromFile,
                    Path = filePath,
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
                
                return new Models.M3ULine
                {
                    IsValid = !string.IsNullOrWhiteSpace(filePath) && fileSystemInfoFile.Exists(),
                    FileSystemFileInfo = fileSystemInfoFile,
                    ReleaseArist = releaseArtist.Replace("_", " ").CleanString(true),
                    TrackNumber = SafeParser.ToNumber<int>(parts[0]),
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
    
}