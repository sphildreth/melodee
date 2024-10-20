using System.Text.RegularExpressions;
using FFMpegCore;
using FFMpegCore.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData;
using Melodee.Plugins.MetaData.Track.Extensions;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using Track = ATL.Track;

namespace Melodee.Plugins.Conversion.Media;

/// <summary>
///     This converts all Media files into MP3 files.
/// </summary>
public sealed partial class MediaConvertor(Configuration configuration) : MetaDataBase(configuration), IConversionPlugin
{
    public override string Id => "61995E53-D998-4BD4-BC83-2AB2F9D9B931";

    public override string DisplayName => nameof(MediaConvertor);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists(directoryInfo))
        {
            return false;
        }

        return FileHelper.IsFileMediaType(fileSystemInfo.Extension(directoryInfo));
    }

    public async Task<OperationResult<FileSystemFileInfo>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        if (!FileHelper.IsFileMediaType(fileSystemInfo.Extension(directoryInfo)))
        {
            return new OperationResult<FileSystemFileInfo>
            {
                Errors = new[]
                {
                    new Exception("Invalid file type. This convertor only processes Media type files.")
                },
                Data = fileSystemInfo
            };
        }

        var fileInfo = new FileInfo(fileSystemInfo.FullName(directoryInfo));
        if (fileInfo.Exists && Configuration.MediaConvertorOptions.ConversionEnabled)
        {
            var fileAtl = new Track(fileSystemInfo.FullName(directoryInfo));
            if (ShouldMediaTrackBeConverted(fileAtl))
            {
                using (Operation.At(LogEventLevel.Debug).Time("Converted [{directoryInfo}] to MP3", fileInfo.FullName))
                {
                    var trackFileInfo = fileAtl.FileInfo();
                    var trackDirectory = trackFileInfo.Directory?.FullName ?? throw new Exception("Invalid FileInfo For Track");
                    var newFileName = Path.Combine(trackDirectory, $"{Path.GetFileNameWithoutExtension(trackFileInfo.Name)}.mp3");

                    await FFMpegArguments.FromFileInput(trackFileInfo)
                        .OutputToFile(newFileName, true, options =>
                        {
                            options.WithAudioBitrate(SafeParser.ToEnum<AudioQuality>(Configuration.MediaConvertorOptions.ConvertBitrate));
                            options.WithAudioSamplingRate(Configuration.MediaConvertorOptions.ConvertSamplingRate);
                            options.WithVariableBitrate(Configuration.MediaConvertorOptions.ConvertVbrLevel);
                            options.WithAudioCodec(AudioCodec.LibMp3Lame).ForceFormat("mp3");
                        }).ProcessAsynchronously();
                    var newAtl = new Track(newFileName);
                    if (string.Equals(newAtl.AudioFormat.ShortName, "mpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        fileInfo = new FileInfo(newFileName);
                        if (Configuration.PluginProcessOptions.DoDeleteOriginal)
                        {
                            trackFileInfo.Delete();
                            Log.Debug($"\u26a0\ufe0f Deleted converted file [{trackFileInfo.FullName}]");
                        }
                        else if (Configuration.PluginProcessOptions.DoRenameConverted)
                        {
                            var movedFileName = Path.Combine(trackFileInfo.DirectoryName!, $"{trackFileInfo.Name}.{ Configuration.PluginProcessOptions.ConvertedExtension }");
                            trackFileInfo.MoveTo(movedFileName);
                            Log.Debug($"\ud83d\ude9b Renamed converted file [{trackFileInfo.Name}] => [{ Path.GetFileName(movedFileName)}]");
                        }
                    }
                    else
                    {
                        throw new Exception($"Unable to convert [{trackFileInfo.FullName}] to MP3");
                    }
                }
            }
        }

        return new OperationResult<FileSystemFileInfo>
        {
            Data = fileInfo.ToFileSystemInfo()
        };
    }

    private static bool ShouldMediaTrackBeConverted(Track track)
    {
        if (track.AudioFormat == null || (track.AudioFormat?.MimeList?.Contains("image") ?? false))
        {
            return false;
        }

        var shortName = track.AudioFormat?.ShortName ?? string.Empty;

        if (MpegRegex().IsMatch(shortName))
        {
            var ext = track.FileInfo().Extension;
            if (ext.ToLower().EndsWith("m4a")) // M4A is an audio file using the MP4 encoding
            {
                return true;
            }

            return false;
        }

        return true;
    }

    [GeneratedRegex("mpeg([0-9]*)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MpegRegex();
}
