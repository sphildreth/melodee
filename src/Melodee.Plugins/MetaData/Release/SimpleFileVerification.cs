using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Discovery;
using Serilog;

namespace Melodee.Plugins.MetaData.Release;

/// <summary>
/// Processes Simple Verification Files (SFV) and gets files (tracks) and files CRC for release.
/// </summary>
public sealed class SimpleFileVerification : ReleaseMetaDataBase, IReleasePlugin
{
    public override string Id => "6C253D42-F176-4A58-A895-C54BEB1F8A5C";
    
    public override string DisplayName => nameof(SimpleFileVerification);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;
  
    public async Task<OperationResult<Common.Models.Release>> ProcessReleaseAsync(Common.Models.Release release, CancellationToken cancellationToken = default)
    {
        var messages = new List<string>(release.Messages);
        var sfv = release.FileInfosForExtension("sfv").ToArray();

        if (!sfv.Any())
        {
            return new OperationResult<Common.Models.Release>("Skipping validation. No SFV file for Release.")
            {
                Data = release
            }; 
        }
        if (sfv.Count() > 1)
        {
            return new OperationResult<Common.Models.Release>
            {
                Data = release,
                Errors = new[]
                {
                    new Exception("More than 1 SFV file found in Release folder. Consider splitting out each Releases files into subfolders.")
                }
            };
        }

        if (release.Tracks != null && !release.Tracks.Any())
        {
            messages.Add("Release has no tracks.");
        }
        else
        {
            // Parser lines from SFV into Models
            var models = await GetModelsFromSfvFile(sfv.First().FullName);

            if (models.Length != 0)
            {
                // Ensure that the release has a track for each line in the SFV
                foreach (var model in models)
                {
                    var trackForModel = release.Tracks?.FirstOrDefault(x => x.FileSystemInfo == model.FileInfo);
                    if (trackForModel == null)
                    {
                        messages.Add($"!! Missing Track For SFV [{model}]");
                        release.Status = ReleaseStatus.Incomplete;
                    }

                    if (!model.IsValid)
                    {
                        messages.Add($"!! Release has invalid tracks. SFV [{model}]");
                    }
                }
                release.ViaPlugins = release.ViaPlugins.Append(nameof(SimpleFileVerification)).ToArray();
                release.Status = messages.Count != 0 ? ReleaseStatus.NeedsAttention : release.Status;
            }
        }

        return new OperationResult<Common.Models.Release>(messages)
        {
            Type = messages.Count != 0 ? OperationResponseType.ValidationFailure : OperationResponseType.Ok,
            Data = release
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
                    FileInfo = new FileInfo(parts[0]),
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