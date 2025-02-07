using System.ComponentModel;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Serilog;

namespace Melodee.Common.Plugins.MetaData.Directory.Models.Extensions;

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
            Artist = new Artist(
                cueSheet.Artist() ?? throw new Exception($"Invalid artist on {nameof(cueSheet)}"),
                cueSheet.Artist().ToNormalizedString() ?? cueSheet.Artist()!,
                cueSheet.Artist().CleanString(true)),
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
            Status = cueSheet.IsValid ? AlbumStatus.Ok : AlbumStatus.Invalid,
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
        return cueSheet.MetaTagValue<string?>(MetaTagIdentifier.AlbumArtist) ?? cueSheet.MetaTagValue<string?>(MetaTagIdentifier.Artist);
    }

    public static string? AlbumTitle(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<string?>(MetaTagIdentifier.Album);
    }

    public static string? Genre(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<string?>(MetaTagIdentifier.Genre);
    }

    public static int? AlbumYear(this CueSheet cueSheet)
    {
        return cueSheet.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumYear) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               cueSheet.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }
}
