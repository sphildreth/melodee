using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData;
using SixLabors.ImageSharp;

namespace Melodee.Plugins.Conversion.Image;

/// <summary>
///     This converts non JPG image into a JPG image.
/// </summary>
public sealed class ImageConvertor(IMelodeeConfiguration configuration) : MetaDataBase(configuration), IConversionPlugin
{
    public override string Id => "8A169045-C650-4DE5-A564-F0E2D28EF07D";

    public override string DisplayName => nameof(ImageConvertor);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists(directoryInfo))
        {
            return false;
        }

        return FileHelper.IsFileImageType(fileSystemInfo.Extension(directoryInfo));
    }

    public async Task<OperationResult<FileSystemFileInfo>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        if (!FileHelper.IsFileImageType(fileSystemInfo.Extension(directoryInfo)))
        {
            return new OperationResult<FileSystemFileInfo>
            {
                Errors = new[]
                {
                    new Exception("Invalid file type. This convertor only processes Image type files.")
                },
                Data = fileSystemInfo
            };
        }

        var fileInfo = new FileInfo(fileSystemInfo.FullName(directoryInfo));
        if (fileInfo.Exists && !string.Equals(".jpg", fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
        {
            var newName = Path.ChangeExtension(fileInfo.FullName, "jpg");
            var convertedBytes = ConvertToJpegFormatViaSixLabors(await File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken));
            await File.WriteAllBytesAsync(newName, convertedBytes, cancellationToken);
            fileInfo.Delete();
            fileInfo = new FileInfo(newName);            
        }

        // TODO resize if needed based on SettingRegistry.ImagingMaximumImageSize
        
        return new OperationResult<FileSystemFileInfo>
        {
            Data = fileInfo.ToFileSystemInfo()
        };
    }

    private static byte[] ConvertToJpegFormatViaSixLabors(ReadOnlySpan<byte> imageBytes)
    {
        using var outStream = new MemoryStream();
        using (var image = SixLabors.ImageSharp.Image.Load(imageBytes))
        {
            image.SaveAsJpeg(outStream);
        }

        return outStream.ToArray();
    }
}
