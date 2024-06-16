using Melodee.Common.Models;
using FileInfo = Melodee.Common.Models.FileInfo;

namespace Melodee.Plugins.Discovery.File;

public sealed class FileDiscoverer : IFileDiscoverer
{
    public string DisplayName => nameof(FileDiscoverer);

    public string Id => "3528BA3F-4130-4913-9C9F-C7F0F8FF2B4D";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;  
    
    public OperationResult<PagedResult<FileInfo>> FileInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest)
    {
        
        
        throw new NotImplementedException();
    }



}