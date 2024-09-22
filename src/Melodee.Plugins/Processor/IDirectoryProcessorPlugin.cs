using Melodee.Common.Models;

namespace Melodee.Plugins.Processor;

public interface IDirectoryProcessorPlugin : IPlugin
{
    event EventHandler<FileSystemDirectoryInfo>? OnDirectoryProcessed;

    event EventHandler<string>? OnProcessingEvent;

    event EventHandler<int>? OnProcessingStart;

    void StopProcessing();

    Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);
}
