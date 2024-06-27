using Melodee.Common.Models;
using Melodee.Common.Utility;

namespace Melodee.Plugins.Discovery.Directories;

public sealed class DirectoriesDiscoverer : IDirectoriesDiscoverer
{
    public string DisplayName => nameof(DirectoriesDiscoverer);

    public string Id => "EBAD92EA-A49E-4502-818D-D6B36B9E5993";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;
    
    public PagedResult<FileSystemDirectoryInfo> DirectoryInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest)
    {
        var data = new List<FileSystemDirectoryInfo>();

        if (directoryInfo.Exists)
        {
            data.AddRange(DirectoryInfosForDirectoryAction(directoryInfo, pagedRequest, 0));
        }
        return new PagedResult<FileSystemDirectoryInfo>
        {
            Type = data.Count != 0 ? OperationResponseType.Ok : OperationResponseType.Error,
            Data = data 
        };
    }

    private IEnumerable<FileSystemDirectoryInfo> DirectoryInfosForDirectoryAction(System.IO.DirectoryInfo dirInfo, PagedRequest pagedRequest, long parentId)
    {
        var result = new List<FileSystemDirectoryInfo>();
        
        var dd = DirectoryInfoForDirectory(dirInfo, pagedRequest, parentId);
        result.Add(dd);
        
        foreach (var directory in dirInfo.EnumerateDirectories(pagedRequest.Filter ?? "*", SearchOption.TopDirectoryOnly)
                     .Skip(pagedRequest.SkipValue)
                     .Take(pagedRequest.TakeValue))
        {
            result.AddRange(DirectoryInfosForDirectoryAction(directory, pagedRequest, dd.UniqueId));
        }
        return result;
    }
    

    private FileSystemDirectoryInfo DirectoryInfoForDirectory(System.IO.DirectoryInfo dirInfo, PagedRequest pagedRequest, long parentId)
    {
        var allExtensionsForChildDirectory = FileHelper.AllFileExtensionsForDirectory(dirInfo)
            .ToArray()
            .Take(pagedRequest.TakeValue)
            .ToArray();
        return (new FileSystemDirectoryInfo
        {
            ParentId = parentId,
            Path = dirInfo.FullName,
            ImageFilesFound = FileHelper.GetNumberOfImageFilesForDirectory(allExtensionsForChildDirectory),
            MusicFilesFound = FileHelper.GetNumberOfMediaFilesForDirectory(allExtensionsForChildDirectory),
            MusicMetaDataFilesFound = FileHelper.GetNumberOfMediaMetaDataFilesForDirectory(allExtensionsForChildDirectory),
            Name = dirInfo.Name,
            TotalItemsFound = FileHelper.GetNumberOfTotalFilesForDirectory(dirInfo)
        });        
    }
    
}