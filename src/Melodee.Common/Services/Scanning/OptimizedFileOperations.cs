using System.Collections.Concurrent;
using System.IO.Hashing;
using Serilog;

namespace Melodee.Common.Services.Scanning;

/// <summary>
/// Optimized file operations for high-performance directory processing
/// </summary>
public static class OptimizedFileOperations
{
    private static readonly SemaphoreSlim FileOperationSemaphore = new(Environment.ProcessorCount * 2);
    private static readonly ConcurrentDictionary<string, DateTime> FileHashCache = new();
    
    /// <summary>
    /// Asynchronously copy files in batches with optimized performance
    /// </summary>
    public static async Task<int> CopyFilesAsync(
        IEnumerable<(string sourcePath, string destinationPath)> filePairs,
        bool deleteOriginal = false,
        int bufferSize = 1024 * 1024, // 1MB buffer
        CancellationToken cancellationToken = default)
    {
        var copiedCount = 0;
        var tasks = new List<Task>();
        
        foreach (var (sourcePath, destinationPath) in filePairs)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
                
            var task = CopyFileWithThrottleAsync(sourcePath, destinationPath, deleteOriginal, bufferSize, cancellationToken);
            tasks.Add(task);
            
            // Process in batches to avoid overwhelming the system
            if (tasks.Count >= Environment.ProcessorCount)
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
                copiedCount += tasks.Count;
                tasks.Clear();
            }
        }
        
        // Process remaining tasks
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
            copiedCount += tasks.Count;
        }
        
        return copiedCount;
    }
    
    /// <summary>
    /// Copy a single file with throttling and optimized buffering
    /// </summary>
    private static async Task CopyFileWithThrottleAsync(
        string sourcePath, 
        string destinationPath, 
        bool deleteOriginal,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        await FileOperationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await CopyFileOptimizedAsync(sourcePath, destinationPath, bufferSize, cancellationToken).ConfigureAwait(false);
            
            if (deleteOriginal)
            {
                try
                {
                    File.Delete(sourcePath);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete original file after copy: {SourcePath}", sourcePath);
                }
            }
        }
        finally
        {
            FileOperationSemaphore.Release();
        }
    }
    
    /// <summary>
    /// Optimized file copy using streams with large buffers
    /// </summary>
    private static async Task CopyFileOptimizedAsync(
        string sourcePath, 
        string destinationPath, 
        int bufferSize,
        CancellationToken cancellationToken)
    {
        // Ensure destination directory exists
        var destinationDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }
        
        // Skip if files are identical
        if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
            return;
            
        var sourceInfo = new FileInfo(sourcePath);
        if (!sourceInfo.Exists)
            return;
            
        // Check if destination exists and has same size/date (quick duplicate check)
        var destInfo = new FileInfo(destinationPath);
        if (destInfo.Exists && destInfo.Length == sourceInfo.Length && 
            Math.Abs((destInfo.LastWriteTime - sourceInfo.LastWriteTime).TotalSeconds) < 2)
        {
            return; // Files appear to be identical
        }
        
        // Use optimized async file copy
        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
        await using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan);
        
        await sourceStream.CopyToAsync(destStream, bufferSize, cancellationToken).ConfigureAwait(false);
        
        // Preserve timestamps
        File.SetLastWriteTime(destinationPath, sourceInfo.LastWriteTime);
        File.SetCreationTime(destinationPath, sourceInfo.CreationTime);
    }
    
    /// <summary>
    /// Batch delete files with parallel processing
    /// </summary>
    public static async Task<int> DeleteFilesAsync(
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken = default)
    {
        var deletedCount = 0;
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount / 2 // I/O bound, use fewer threads
        };
        
        await Task.Run(() =>
        {
            Parallel.ForEach(filePaths, parallelOptions, filePath =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Interlocked.Increment(ref deletedCount);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete file: {FilePath}", filePath);
                }
            });
        }, cancellationToken).ConfigureAwait(false);
        
        return deletedCount;
    }
    
    /// <summary>
    /// Check if file has changed using cached hash comparison
    /// </summary>
    public static bool HasFileChanged(string filePath, DateTime? lastProcessDate = null)
    {
        if (!File.Exists(filePath))
            return false;
            
        var fileInfo = new FileInfo(filePath);
        
        // Quick date check first
        if (lastProcessDate.HasValue && fileInfo.LastWriteTime <= lastProcessDate.Value)
            return false;
            
        // Use cached hash for more accurate comparison
        var cacheKey = $"{filePath}:{fileInfo.Length}:{fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
        
        return !FileHashCache.ContainsKey(cacheKey);
    }
    
    /// <summary>
    /// Update file hash cache
    /// </summary>
    public static void UpdateFileHashCache(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return;
                
            var fileInfo = new FileInfo(filePath);
            var cacheKey = $"{filePath}:{fileInfo.Length}:{fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
            
            FileHashCache.TryAdd(cacheKey, DateTime.UtcNow);
            
            // Clean old cache entries periodically
            if (FileHashCache.Count > 10000)
            {
                var cutoff = DateTime.UtcNow.AddHours(-1);
                var keysToRemove = FileHashCache
                    .Where(kvp => kvp.Value < cutoff)
                    .Select(kvp => kvp.Key)
                    .ToList();
                    
                foreach (var key in keysToRemove)
                {
                    FileHashCache.TryRemove(key, out _);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to update file hash cache for: {FilePath}", filePath);
        }
    }
    
    /// <summary>
    /// Efficiently enumerate files with lazy loading
    /// </summary>
    public static IAsyncEnumerable<FileInfo> EnumerateFilesAsync(
        string directoryPath,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        CancellationToken cancellationToken = default)
    {
        return EnumerateFilesAsyncImpl(directoryPath, searchPattern, searchOption, cancellationToken);
    }
    
    private static async IAsyncEnumerable<FileInfo> EnumerateFilesAsyncImpl(
        string directoryPath,
        string searchPattern,
        SearchOption searchOption,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directoryPath))
            yield break;
            
        await Task.Yield(); // Allow other operations to proceed
        
        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = searchOption == SearchOption.AllDirectories,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false
        };
        
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, searchPattern, enumerationOptions))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(filePath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get file info for: {FilePath}", filePath);
                continue;
            }
            
            yield return fileInfo;
            
            // Yield periodically to allow other operations
            if (Random.Shared.Next(100) == 0)
            {
                await Task.Yield();
            }
        }
    }
    
    /// <summary>
    /// Write text to file asynchronously with retry logic
    /// </summary>
    public static async Task WriteTextFileAsync(
        string filePath,
        string content,
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, content, cancellationToken).ConfigureAwait(false);
                UpdateFileHashCache(filePath);
                return;
            }
            catch (IOException) when (retryCount < maxRetries - 1)
            {
                retryCount++;
                await Task.Delay(100 * retryCount, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
