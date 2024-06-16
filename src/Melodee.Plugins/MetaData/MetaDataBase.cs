using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData;

public abstract class MetaDataBase : IFilePlugin
{
    public abstract string Id { get; }
    
    public abstract string DisplayName { get; }
    
    public abstract bool IsEnabled { get; set; }
    
    public abstract int SortOrder { get; }
    
    public bool StopProcessing { get; internal set; }
    
    public abstract bool DoesHandleFile(FileSystemInfo fileSystemInfo);
    
    public abstract Task<OperationResult<Release>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default);
}