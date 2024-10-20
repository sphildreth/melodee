namespace Melodee.Common.Models.Extensions;

public static class ImageInfoExtensions
{
    public static bool IsCrcHashMatch(this ImageInfo imageInfo, string crcHash)
    {
        return string.Equals(imageInfo.CrcHash, crcHash, StringComparison.OrdinalIgnoreCase);
    }
}
