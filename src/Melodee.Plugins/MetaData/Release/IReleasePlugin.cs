using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Release;

public interface IReleasePlugin : IPlugin
{
    bool StopProcessing { get; }
    
    Task<OperationResult<Common.Models.Release>> ProcessReleaseAsync(Common.Models.Release release, CancellationToken cancellationToken = default);
}