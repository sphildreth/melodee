using Melodee.Common.Models;

namespace Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers;

public sealed class PMediaHandler : INfoHandler
{
    public async Task<bool> IsHandlerForNfoAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        var fileContents = await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
        if (fileContents.Length > 0)
        {
            if (fileContents.Contains("1BPEgheWBbFzhq3zZephEjESaJVvewBU5R") ||
                fileContents.Contains("PMEDIA GROUP"))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<Album?> HandleNfoAsync(FileInfo fileInfo, bool doDeleteOriginal,
        CancellationToken cancellationToken = default)
    {
        var isPMediaNfo = await IsHandlerForNfoAsync(fileInfo, cancellationToken);
        if (isPMediaNfo && fileInfo.DirectoryName != null && doDeleteOriginal)
        {
            var coverFileName = Path.Combine(fileInfo.DirectoryName, "cover.jpg");
            if (File.Exists(coverFileName))
            {
                File.Delete(coverFileName);
            }

            fileInfo.Delete();
        }

        return null;
    }
}
