using System.Diagnostics;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Extensions;

public static class HttpClientExtensions
{
    /// <summary>
    /// Download given url to a file and if a file exists with same name execute the condition.
    /// </summary>
    /// <returns>True if the file downloaded was kept, false if deleted as failed condition or errored.</returns>
    public static async Task<bool> DownloadFileAsync(this HttpClient httpClient, string url, string filePath, Func<FileInfo, FileInfo, CancellationToken, Task<bool>>? overrideCondition = null, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filePath);
        var tempDownloadName = Path.Combine(fileInfo.DirectoryName!, $"{Guid.NewGuid()}{fileInfo.Extension}");

        try
        {
            using (Operation.At(LogEventLevel.Debug).Time("\u2584 Downloaded url [{Url}] to file [{File}]", url, filePath))
            {
                await using (var stream = await httpClient.GetStreamAsync(url, cancellationToken))
                {
                    await using (var fs = new FileStream(tempDownloadName, FileMode.OpenOrCreate))
                    {
                        await stream.CopyToAsync(fs, cancellationToken);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Attempting to download [{url}] to file [{filePath}], [{e}]");
            return false;
        }
        if (fileInfo.Exists && overrideCondition == null)
        {
            fileInfo.Delete();
            File.Move(tempDownloadName, filePath);
        }
        if (fileInfo.Exists && overrideCondition != null && await overrideCondition(fileInfo, new FileInfo(tempDownloadName), cancellationToken))
        {
            fileInfo.Delete();
            File.Move(tempDownloadName, filePath);
        } 
        else if (fileInfo.Exists && overrideCondition != null && !await overrideCondition(fileInfo, new FileInfo(tempDownloadName), cancellationToken))
        {
            File.Delete(tempDownloadName);
            return false;
        }
        else
        {
            File.Move(tempDownloadName, filePath);
        }
        return true;
    }
}
