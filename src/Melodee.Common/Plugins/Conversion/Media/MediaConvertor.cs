using System.Text.RegularExpressions;
using FFMpegCore;
using FFMpegCore.Enums;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData;
using Melodee.Common.Plugins.MetaData.Song.Extensions;
using Melodee.Common.Utility;
using Serilog.Events;
using SerilogTimings;
using Track = ATL.Track;

namespace Melodee.Common.Plugins.Conversion.Media;

/// <summary>
///     This converts all Media files into MP3 files.
/// </summary>
public sealed partial class MediaConvertor(IMelodeeConfiguration configuration) : MetaDataBase(configuration), IConversionPlugin
{
    public override string Id => "61995E53-D998-4BD4-BC83-2AB2F9D9B931";

    public override string DisplayName => nameof(MediaConvertor);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists(directoryInfo) || !MelodeeConfiguration.GetValue<bool>(SettingRegistry.ConversionEnabled))
        {
            return false;
        }

        return FileHelper.IsFileMediaType(fileSystemInfo.Extension(directoryInfo));
    }

    public async Task<OperationResult<FileSystemFileInfo>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        if (!MelodeeConfiguration.GetValue<bool>(SettingRegistry.ConversionEnabled))
        {
            return new OperationResult<FileSystemFileInfo>($"Configuration value '{SettingRegistry.ConversionEnabled}' has disabled media conversion.")
            {
                Data = fileSystemInfo,
                Type = OperationResponseType.NotImplementedOrDisabled
            };
        }

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
        if (fileInfo.Exists && SafeParser.ToBoolean(Configuration[SettingRegistry.ConversionEnabled]))
        {
            var fileAtl = new Track(fileSystemInfo.FullName(directoryInfo));
            if (ShouldMediaSongBeConverted(fileAtl))
            {
                using (Operation.At(LogEventLevel.Debug).Time("Converted [{directoryInfo}] to MP3", fileInfo.FullName))
                {
                    var songFileInfo = fileAtl.FileInfo();
                    var songDirectory = songFileInfo.Directory?.FullName ?? throw new Exception("Invalid FileInfo For Song");
                    var newFileName = Path.Combine(songDirectory, $"{Path.GetFileNameWithoutExtension(songFileInfo.Name)}.mp3");

                    await FFMpegArguments.FromFileInput(songFileInfo)
                        .OutputToFile(newFileName, true, options =>
                        {
                            options.WithAudioBitrate(SafeParser.ToEnum<AudioQuality>(Configuration[SettingRegistry.ConversionBitrate]));
                            options.WithAudioSamplingRate(SafeParser.ToNumber<int>(Configuration[SettingRegistry.ConversionSamplingRate]));
                            options.WithVariableBitrate(SafeParser.ToNumber<int>(Configuration[SettingRegistry.ConversionVbrLevel]));
                            options.WithAudioCodec(AudioCodec.LibMp3Lame).ForceFormat("mp3");
                        }).ProcessAsynchronously();
                    var newAtl = new Track(newFileName);
                    if (string.Equals(newAtl.AudioFormat.ShortName, "mpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        var convertedRenamedExtension = SafeParser.ToString(Configuration[SettingRegistry.ProcessingConvertedExtension]);
                        fileInfo = new FileInfo(newFileName);
                        if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                        {
                            songFileInfo.Delete();
                        }
                        else if (convertedRenamedExtension.Nullify() != null)
                        {
                            var movedFileName = Path.Combine(songFileInfo.DirectoryName!, $"{songFileInfo.Name}.{convertedRenamedExtension}");
                            songFileInfo.MoveTo(movedFileName);
                        }
                    }
                    else
                    {
                        throw new Exception($"Unable to convert [{songFileInfo.FullName}] to MP3");
                    }
                }
            }
        }

        return new OperationResult<FileSystemFileInfo>
        {
            Data = fileInfo.ToFileSystemInfo()
        };
    }

    private static bool ShouldMediaSongBeConverted(Track song)
    {
        if (song.AudioFormat == null || (song.AudioFormat?.MimeList?.Contains("image") ?? false))
        {
            return false;
        }

        var shortName = song.AudioFormat?.ShortName ?? string.Empty;

        if (MpegRegex().IsMatch(shortName))
        {
            var ext = song.FileInfo().Extension;
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
