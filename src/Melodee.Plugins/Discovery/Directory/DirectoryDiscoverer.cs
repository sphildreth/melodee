using Melodee.Common.Models;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Plugins.Discovery.Directory;

public sealed class DirectoryDiscoverer : IDirectoryDiscoverer
{
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
                var allExtensionsForDirectory = FileHelper.AllFileExtensionsForDirectory(directory).ToArray();
                var dirInfo = new DirectoryInfo
                {
                    Path = directory.FullName,
                    ImageFilesFound = FileHelper.GetNumberOfImageFilesForDirectory(allExtensionsForDirectory),
                    MusicFilesFound = FileHelper.GetNumberOfMediaFilesForDirectory(allExtensionsForDirectory),
                    MusicMetaDataFilesFound = FileHelper.GetNumberOfMediaMetaDataFilesForDirectory(allExtensionsForDirectory),
                    ShortName = directory.Name,
                    TotalItemsFound = FileHelper.GetNumberOfTotalFilesForDirectory(directory)
                };
                foreach (var childDir in directory.EnumerateDirectories(pagedRequest.Filter ?? "*",
                             SearchOption.AllDirectories))
                {
                    var allExtensionsForChildDirectory = FileHelper.AllFileExtensionsForDirectory(childDir).ToArray();
                    data.Add(new DirectoryInfo
                    {
                        ParentId = dirInfo.Id,
                        Path = childDir.FullName,
                        ImageFilesFound = FileHelper.GetNumberOfImageFilesForDirectory(allExtensionsForChildDirectory),
                        MusicFilesFound = FileHelper.GetNumberOfMediaFilesForDirectory(allExtensionsForChildDirectory),
                        MusicMetaDataFilesFound = FileHelper.GetNumberOfMediaMetaDataFilesForDirectory(allExtensionsForChildDirectory),
                        ShortName = childDir.Name,
                        TotalItemsFound = FileHelper.GetNumberOfTotalFilesForDirectory(childDir)
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

}