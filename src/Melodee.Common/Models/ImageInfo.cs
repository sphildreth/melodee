using Melodee.Common.Enums;

namespace Melodee.Common.Models;

public sealed record ImageInfo
{
    public PictureIdentifier PictureIdentifier { get; init; }

    public byte[]? Bytes  { get; init; }
    
    public int Width { get; init; }

    public int Height { get; init; }

    public int ColorDepth { get; init; }
    
    public int NumColorsInPalette { get; init; }
    
    public int SortOrder  { get; init; }
}