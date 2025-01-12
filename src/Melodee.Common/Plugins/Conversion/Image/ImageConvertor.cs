using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData;
using Melodee.Common.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Melodee.Common.Plugins.Conversion.Image;

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
        if (fileInfo.Exists)
        {
            var smallImageSize = MelodeeConfiguration.GetValue<int>(SettingRegistry.ImagingSmallSize);
            var mediumImageSize = MelodeeConfiguration.GetValue<int>(SettingRegistry.ImagingMediumSize);
            var largeImageSize = MelodeeConfiguration.GetValue<int>(SettingRegistry.ImagingLargeSize);

            var newName = Path.ChangeExtension(fileInfo.FullName, "jpg");
            var imageInfo = await SixLabors.ImageSharp.Image.IdentifyAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);

            var larger = imageInfo.Width;
            if (larger < smallImageSize)
            {
                larger = smallImageSize;
            }

            if (larger < imageInfo.Height)
            {
                larger = imageInfo.Height;
            }

            var resizeWithPaddingSize = smallImageSize;
            if (larger > smallImageSize)
            {
                resizeWithPaddingSize = mediumImageSize;
            }

            if (larger > mediumImageSize)
            {
                resizeWithPaddingSize = largeImageSize;
            }

            var didModify = false;
            var imageBytes = await File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken);
            if (!string.Equals(".jpg", fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
            {
                imageBytes = ConvertToJpegFormatViaSixLabors(imageBytes);
                didModify = true;
            }

            if (imageInfo.Width != imageInfo.Height || imageInfo.Height > largeImageSize)
            {
                imageBytes = ResizeAndPadToBeSquare(imageBytes, resizeWithPaddingSize);
                didModify = true;
            }

            if (didModify)
            {
                await File.WriteAllBytesAsync(newName, imageBytes, cancellationToken);
                if (newName != fileInfo.FullName)
                {
                    fileInfo.Delete();
                    fileInfo = new FileInfo(newName);
                }
            }
        }

        return new OperationResult<FileSystemFileInfo>
        {
            Data = fileInfo.ToFileSystemInfo()
        };
    }

    public static byte[] ResizeAndPadToBeSquare(ReadOnlySpan<byte> imageBytes, int width)
    {
        if (imageBytes.Length == 0)
        {
            return imageBytes.ToArray();
        }

        using var outStream = new MemoryStream();
        using (var image = SixLabors.ImageSharp.Image.Load(imageBytes))
        {
            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Size = new Size(width, width),
                    Mode = ResizeMode.Pad
                }).BackgroundColor(new Rgba32(255, 255, 255, 0)));
            image.SaveAsJpeg(outStream);
        }

        return outStream.ToArray();
    }

    public static byte[] ResizeImageIfNeeded(ReadOnlySpan<byte> imageBytes, int maxWidth, int maxHeight, bool isForUserAvatar)
    {
        if (imageBytes.Length == 0)
        {
            return imageBytes.ToArray();
        }

        using var outStream = new MemoryStream();
        using (var image = SixLabors.ImageSharp.Image.Load(imageBytes))
        {
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                image.Mutate(x => x.Resize(maxWidth, maxHeight));
            }

            if (isForUserAvatar)
            {
                image.SaveAsGif(outStream);
            }
            else
            {
                image.SaveAsJpeg(outStream);
            }
        }

        return outStream.ToArray();
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
