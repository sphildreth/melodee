using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Utility;
using SixLabors.ImageSharp;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Common.Plugins.Extensions;

public static class FileSystemDirectoryInfoExtensions
{
    public static async Task<ImageInfo[]> ImagesForTypeAsync(this FileSystemDirectoryInfo directory, int maxNumberOfImagesAllowed, PictureIdentifier[] forPictureIdentifiers, IImageValidator imageValidator, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(directory.FullName(), SearchOption.TopDirectoryOnly).ToArray();
        var index = 1;
        var maxNumberOfImagesLength = SafeParser.ToNumber<short>(maxNumberOfImagesAllowed.ToString().Length);
        foreach (var imageFile in imageFiles.Order())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileInfo = new FileInfo(imageFile);
            var fileNameNormalized = fileInfo.Name.ToNormalizedString() ?? fileInfo.Name;
            if (ImageHelper.IsArtistImage(fileInfo) || ImageHelper.IsArtistSecondaryImage(fileInfo))
            {
                if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsArtistImage(fileInfo) ? PictureIdentifier.Artist : PictureIdentifier.ArtistSecondary, cancellationToken)).Data.IsValid)
                {
                    continue;
                }

                var pictureIdentifier = PictureIdentifier.NotSet;
                if (ImageHelper.IsArtistImage(fileInfo))
                {
                    pictureIdentifier = PictureIdentifier.Band;
                }
                else if (ImageHelper.IsArtistSecondaryImage(fileInfo))
                {
                    pictureIdentifier = PictureIdentifier.BandSecondary;
                }

                if (forPictureIdentifiers.Contains(pictureIdentifier))
                {
                    var imageInfo = await Image.LoadAsync(fileInfo.FullName, cancellationToken);
                    var fileInfoFileSystemInfo = fileInfo.ToFileSystemInfo();
                    imageInfos.Add(new ImageInfo
                    {
                        CrcHash = Crc32.Calculate(fileInfo),
                        FileInfo = new FileSystemFileInfo
                        {
                            Name = $"{ImageInfo.ImageFilePrefix}{index.ToStringPadLeft(maxNumberOfImagesLength)}-{pictureIdentifier}.jpg",
                            Size = fileInfoFileSystemInfo.Size,
                            OriginalName = fileInfo.Name
                        },
                        OriginalFilename = fileInfo.Name,
                        PictureIdentifier = pictureIdentifier,
                        Width = imageInfo.Width,
                        Height = imageInfo.Height,
                        SortOrder = index
                    });
                    index++;
                }
            }
        }

        return imageInfos.ToArray();
    }
}
