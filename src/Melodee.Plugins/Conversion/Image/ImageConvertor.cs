using Melodee.Common.Models;
using Melodee.Plugins.Conversion.Models;
using Melodee.Plugins.Discovery;
using Melodee.Plugins.MetaData;
using SixLabors.ImageSharp;

namespace Melodee.Plugins.Conversion.Image;

/// <summary>
///     This converts non JPG images into a JPG images.
/// </summary>
public sealed class ImageConvertor : MetaDataBase, IConversionPlugin
{
    public override string Id => "8A169045-C650-4DE5-A564-F0E2D28EF07D";

    public override string DisplayName => nameof(ImageConvertor);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists)
        {
            return false;
        }
        return FileHelper.IsFileImageType(fileSystemInfo.Extension);
    }

    public async Task<OperationResult<FileSystemInfo>> ProcessFileAsync(FileSystemInfo fileSystemInfo, ProcessFileOptions processFileOptions, CancellationToken cancellationToken = default)
    {
        if (!FileHelper.IsFileImageType(fileSystemInfo.Extension))
        {
            return new OperationResult<FileSystemInfo>
            {
                Errors = new[]
                {
                    new Exception("Invalid file type. This convertor only processes Image type files.")
                },
                Data = fileSystemInfo
            };
        }

        var fileInfo = new FileInfo(fileSystemInfo.FullName);
        if (fileInfo.Exists && !string.Equals("jpg", fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
        {
            var newName = Path.ChangeExtension(fileInfo.FullName, "jpg");
            var convertedBytes = ConvertToJpegFormatViaSixLabors(await File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken));
            await File.WriteAllBytesAsync(newName, convertedBytes, cancellationToken);
            fileInfo = new FileInfo(newName);
            if (processFileOptions.DoDeleteOriginal)
            {
                fileInfo.Delete();
            }
        }

        return new OperationResult<FileSystemInfo>
        {
            Data = fileInfo
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