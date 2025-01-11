using Melodee.Common.Models;

namespace Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers;

public interface INfoHandler
{
    Task<bool> IsHandlerForNfoAsync(FileInfo fileInfo, CancellationToken cancellationToken = default);
    Task<Album?> HandleNfoAsync(FileInfo fileInfo, bool doDeleteOriginal, CancellationToken cancellationToken = default);
}
