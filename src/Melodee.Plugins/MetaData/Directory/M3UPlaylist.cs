using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Plugins.MetaData.Directory;

public sealed class M3UPlaylist(Configuration configuration) : ReleaseMetaDataBase(configuration), IDirectoryPlugin
{
    public override string Id => "800EBFEF-4A9A-4DD8-8505-056D13535D45";
    
    public override string DisplayName => nameof(M3UPlaylist);

    public override bool IsEnabled { get; set; } = false;

    public override int SortOrder { get; } = 0;

    public async Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        // var messages = new List<string>(directoryInfo.Messages);
        // var m3U = directoryInfo.FileInfosForExtension("m3u").ToArray();
        //
        // if (!m3U.Any())
        // {
        //     return new OperationResult<Common.Models.Release>("Skipping validation. No M3U file for Release.")
        //     {
        //         Data = directoryInfo
        //     }; 
        // }
        // if (m3U.Count() > 1)
        // {
        //     return new OperationResult<Common.Models.Release>
        //     {
        //         Data = directoryInfo,
        //         Errors = new[]
        //         {
        //             new Exception("More than 1 M3U file found in Release folder. Consider splitting out each Releases files into subfolders.")
        //         }
        //     };
        // }
        //
        // if (directoryInfo.Tracks != null && !directoryInfo.Tracks.Any())
        // {
        //     messages.Add("Release has no tracks.");
        // }
        // else
        // {
        //     // Parser lines from M3U into Models
        //     var models = await GetModelsFromM3UFile(m3U.First().FullName);
        //
        //     if (models.Length != 0)
        //     {
        //         // Ensure that the release has a track for each line in the M3U
        //         foreach (var model in models)
        //         {
        //             var trackForModel = directoryInfo.Tracks?.FirstOrDefault(x => x.FileSystemInfo == model.FileInfo);
        //             if (trackForModel == null)
        //             {
        //                 messages.Add($"!! Missing Track For M3U [{model}]");
        //                 directoryInfo.Status = ReleaseStatus.Incomplete;
        //             }
        //
        //             if (!model.IsValid)
        //             {
        //                 messages.Add($"!! Release has invalid tracks. M3U [{model}]");
        //             }
        //         }
        //         directoryInfo.ViaPlugins = directoryInfo.ViaPlugins.Append(nameof(SimpleFileVerification)).ToArray();
        //         directoryInfo.Status = messages.Count != 0 ? ReleaseStatus.NeedsAttention : directoryInfo.Status;
        //     }
        // }
        //
        // return new OperationResult<Common.Models.Release>(messages)
        // {
        //     Type = messages.Count != 0 ? OperationResponseType.ValidationFailure : OperationResponseType.Ok,
        //     Data = directoryInfo
        // };

        throw new NotImplementedException();
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
            if (parts.Length == 3)
            {
                var releaseArtist = parts[1];
                string trackTitle = parts[2];
                
                string? dirName = null;
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var fi = new FileInfo(filePath);
                    dirName = fi.DirectoryName!;
                }
                
                return new Models.M3ULine
                {
                    IsValid = File.Exists(Path.Combine(dirName ?? string.Empty, lineFromFile)),
                    FileInfo = new FileInfo(lineFromFile),
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