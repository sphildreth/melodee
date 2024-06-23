using Melodee.Common.Models;
using Melodee.Plugins.Discovery;
using Melodee.Plugins.MetaData;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Melodee.Plugins.Conversion.Image;

/// <summary>
/// This converts non JPG images into a JPG images.
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

    public Task<OperationResult<FileSystemInfo>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        // If the file isn't jpg convert to jpg and return new FileSystemInfo pointing to jpg file.
        
        
        
        throw new NotImplementedException();
    }
    
    
    
    public static byte[] ConvertToJpegFormatViaSixLabors(ReadOnlySpan<byte> imageBytes)
    {
        using (var outStream = new MemoryStream())
        {
            using(var image = SixLabors.ImageSharp.Image.Load(imageBytes))
            {
                image.SaveAsJpeg(outStream);
            }
            return outStream.ToArray();
        }
    }  
}