using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Microsoft.VisualBasic;

namespace Melodee.Common.Models;

[Serializable]
public sealed record ImageInfo
{
    /// <summary>
    /// May not exist if the image was extracted from tag.
    /// </summary>
    public FileSystemFileInfo? FileInfo { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PictureIdentifier PictureIdentifier { get; init; }
    
    public required string CrcHash { get; init; }
    
    public int Width { get; init; }

    public int Height { get; init; }
    
    public int SortOrder  { get; init; }
    
    public bool WasEmbeddedInTrack { get; init; }
}