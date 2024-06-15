using Melodee.Common.Models;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;
// ReSharper disable MemberCanBePrivate.Global

namespace Melodee.Plugins.Discovery.Directory;

public sealed class DirectoryDiscoverer : IDirectoryDiscoverer
{
    public static readonly IEnumerable<string> MediaMetaDataFileTypeExtensions =
    [
        "m3u",
        "sfv"
    ];
    
    public static readonly IEnumerable<string> MediaFileTypeExtensions =
    [
        "aac",
        "ac3",
        "aiff",
        "ape",
        "flac",
        "mp3",
        "ogg",
        "sfu",
        "svg",
        "wav",
        "wma"
    ];
    
    public static readonly IEnumerable<string> ImageFileTypeExtensions =
    [
        "bmp",
        "gif",
        "jfif",
        "jpeg",
        "jpg",
        "png",
        "tiff",
        "webp"
    ];    

    public string DisplayName => nameof(DirectoryDiscoverer);

    public string Id => "EBAD92EA-A49E-4502-818D-D6B36B9E5993";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;
    
    public OperationResult<PagedResult<DirectoryInfo>> DirectoryInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest)
    {
        var data = new List<DirectoryInfo>();

        if (directoryInfo.Exists)
        {
            foreach (var directory in directoryInfo.EnumerateDirectories(pagedRequest.Filter ?? "*",
                         SearchOption.AllDirectories))
            {
                var allExtensionsForDirectory = AllFileExtensionsForDirectory(directory).ToArray();
                var dirInfo = new DirectoryInfo
                {
                    Path = directory.FullName,
                    ImageFilesFound = GetNumberOfImageFilesForDirectory(allExtensionsForDirectory),
                    MusicFilesFound = GetNumberOfMediaFilesForDirectory(allExtensionsForDirectory),
                    MusicMetaDataFilesFound = GetNumberOfMediaMetaDataFilesForDirectory(allExtensionsForDirectory),
                    ShortName = directory.Name,
                    TotalItemsFound = GetNumberOfTotalFilesForDirectory(directory)
                };
                foreach (var childDir in directory.EnumerateDirectories(pagedRequest.Filter ?? "*",
                             SearchOption.AllDirectories))
                {
                    var allExtensionsForChildDirectory = AllFileExtensionsForDirectory(childDir).ToArray();
                    data.Add(new DirectoryInfo
                    {
                        ParentId = dirInfo.Id,
                        Path = childDir.FullName,
                        ImageFilesFound = GetNumberOfImageFilesForDirectory(allExtensionsForChildDirectory),
                        MusicFilesFound = GetNumberOfMediaFilesForDirectory(allExtensionsForChildDirectory),
                        MusicMetaDataFilesFound = GetNumberOfMediaMetaDataFilesForDirectory(allExtensionsForChildDirectory),
                        ShortName = childDir.Name,
                        TotalItemsFound = GetNumberOfTotalFilesForDirectory(childDir)
                    });
                }

                data.Add(dirInfo);
            }
        }

        return new OperationResult<PagedResult<DirectoryInfo>>
        {
            IsSuccess = data.Count != 0,
            Data = new PagedResult<DirectoryInfo> { Rows = data }
        };
    }

    private static int GetNumberOfTotalFilesForDirectory(System.IO.DirectoryInfo directoryInfo)
    {
        return directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Count();        
    }

    private static IEnumerable<IGrouping<string, System.IO.FileInfo>> AllFileExtensionsForDirectory(
        System.IO.DirectoryInfo directoryInfo)
    {
        return directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).GroupBy(x => x.Extension);
    }
    
    private static int GetNumberOfMediaFilesForDirectory(IEnumerable<IGrouping<string, System.IO.FileInfo>> fileExtensions)
    {
        var result = 0;

        foreach (var extGroup in fileExtensions)
        {
            if (IsFileMediaType(extGroup.Key))
            {
                result += extGroup.Count();
            }
        }
        return result;
    }
    
    private static int GetNumberOfImageFilesForDirectory(IEnumerable<IGrouping<string, System.IO.FileInfo>> fileExtensions)
    {
        var result = 0;
        foreach (var extGroup in fileExtensions)
        {
            if (IsFileImageType(extGroup.Key))
            {
                result += extGroup.Count();
            }
        }
        return result;
    }    
    
    private static int GetNumberOfMediaMetaDataFilesForDirectory(IEnumerable<IGrouping<string, System.IO.FileInfo>> fileExtensions)
    {
        var result = 0;
        foreach (var extGroup in fileExtensions)
        {
            if (IsFileMediaMetaDataType(extGroup.Key))
            {
                result += extGroup.Count();
            }
        }
        return result;
    }  

    public static bool IsFileMediaType(string? extension) => !string.IsNullOrEmpty(extension) && MediaFileTypeExtensions.Contains(extension.Replace(".", ""), StringComparer.OrdinalIgnoreCase);
    
    public static bool IsFileImageType(string? extension) => !string.IsNullOrEmpty(extension) && ImageFileTypeExtensions.Contains(extension.Replace(".", ""), StringComparer.OrdinalIgnoreCase);
    
    public static bool IsFileMediaMetaDataType(string? extension) => !string.IsNullOrEmpty(extension) && MediaMetaDataFileTypeExtensions.Contains(extension.Replace(".", ""), StringComparer.OrdinalIgnoreCase);   
}