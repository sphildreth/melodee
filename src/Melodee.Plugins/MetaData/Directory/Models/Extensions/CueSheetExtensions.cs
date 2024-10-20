using System.ComponentModel;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Serilog;

namespace Melodee.Plugins.MetaData.Directory.Models.Extensions;

public static class CueSheetExtensions
{
    private static ReleaseFile ToReleaseFile(this CueSheet cueSheet)
    {
        return new ReleaseFile
        {
            FileSystemFileInfo = cueSheet.MediaFileSystemFileInfo,
            ReleaseFileType = ReleaseFileType.MetaData,
            ProcessedByPlugin = nameof(Directory.CueSheet)
        };
    }

    public static Release ToRelease(this CueSheet cueSheet, FileSystemDirectoryInfo directoryInfo)
    {
        var fileInfo = new FileInfo(cueSheet.MediaFileSystemFileInfo.FullName(directoryInfo));
        return new Release
        {
            Files = new[]
            {
                cueSheet.ToReleaseFile()
            },
            ViaPlugins = new string[1]
            {
                nameof(Directory.CueSheet)
            },
            Tags = cueSheet.Tags,
            Tracks = cueSheet.Tracks,
            Status = cueSheet.IsValid ? ReleaseStatus.Complete : ReleaseStatus.Incomplete,
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                Path = fileInfo.Directory!.FullName,
                Name = fileInfo.Directory!.Name
            },
            Images = null
        };
    }

    private static T? MetaTagValue<T>(this CueSheet cueSheet, MetaTagIdentifier metaTagIdentifier)
    {
        var d = default(T?);
        if (!cueSheet.Tags.Any())
        {
            return d;
        }

        try
        {
            var vv = cueSheet.Tags.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T?));
            return (T?)converter.ConvertFrom(vv);
        }
        catch (Exception e)
        {
            Log.Debug(e, "CueSheet [{CueSheet}]", cueSheet);
        }

        return d;
    }

    public static string? Artist(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<string?>(MetaTagIdentifier.Artist);
    }

    public static string? ReleaseTitle(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<string?>(MetaTagIdentifier.Album);
    }

    public static int TrackTotalNumber(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.TrackTotal) ?? 0;
    }

    public static int MediaNumber(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.DiscNumber) ?? 0;
    }

    public static int MediaCountValue(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.DiscNumberTotal) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.DiscTotal) ??
               0;
    }

    public static int? ReleaseYear(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseYear) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }
}
