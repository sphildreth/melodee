using Melodee.Common.Models;
using Melodee.Plugins.Discovery;

namespace Melodee.Plugins.MetaData;

public sealed class MetaTag
{
    public bool HandlesFile(string fullFilename) => FileHelper.IsFileMediaType(Path.GetExtension(fullFilename));

    public Task<OperationResult<Release>> ProcessFile(string file, CancellationToken cancellationToken = default)
    {
        
        
        throw new NotImplementedException();
    }
}