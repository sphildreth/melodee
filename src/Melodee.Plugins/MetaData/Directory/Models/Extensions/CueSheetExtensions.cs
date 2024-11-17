using System.ComponentModel;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Serilog;

namespace Melodee.Plugins.MetaData.Directory.Models.Extensions;

public static class CueSheetExtensions
{
    private static AlbumFile ToAlbumFile(this CueSheet cueSheet)
    {
        return new AlbumFile
        {
            FileSystemFileInfo = cueSheet.MediaFileSystemFileInfo,
            AlbumFileType = AlbumFileType.MetaData,
            ProcessedByPlugin = nameof(Directory.CueSheet)
        };
    }

    public static Album ToAlbum(this CueSheet cueSheet, FileSystemDirectoryInfo directoryInfo)
    {
        var fileInfo = new FileInfo(cueSheet.MediaFileSystemFileInfo.FullName(directoryInfo));
        return new Album
        {
            Directory = directoryInfo,
            Files = new[]
            {
                cueSheet.ToAlbumFile()
            },
            ViaPlugins = new string[1]
            {
                nameof(Directory.CueSheet)
            },
            Tags = cueSheet.Tags,
            Songs = cueSheet.Songs,
            Status = cueSheet.IsValid ? AlbumStatus.Ok : AlbumStatus.NeedsAttention,
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

    public static string? AlbumTitle(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<string?>(MetaTagIdentifier.Album);
    }

    public static int SongTotalNumber(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.SongTotal) ?? 0;
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

    public static int? AlbumYear(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumYear) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }
}
