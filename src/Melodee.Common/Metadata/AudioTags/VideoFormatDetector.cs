namespace Melodee.Common.Metadata.AudioTags;

/// <summary>
/// Detects if a file is a video file based on common video signatures and extensions.
/// </summary>
public static class VideoFormatDetector
{
    private static readonly string[] VideoExtensions = [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".3gp"];

    /// <summary>
    /// Checks if the specified file is likely a video file.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the file is detected as a video file, otherwise false.</returns>
    public static async Task<bool> IsVideoFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Quick check based on extension
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (Array.Exists(VideoExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
        {
            // For common video extensions, do additional signature verification
            return await HasVideoSignatureAsync(filePath, extension, cancellationToken);
        }
        
        // For unknown extensions, check signature only
        return await HasVideoSignatureAsync(filePath, extension, cancellationToken);
    }

    private static async Task<bool> HasVideoSignatureAsync(string filePath, string extension, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var buffer = new byte[16];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            
            if (bytesRead < 4)
            {
                return false;
            }

            // MP4/MOV/M4V family (check for ftyp box with video codecs)
            if (bytesRead >= 8 && buffer[4] == 'f' && buffer[5] == 't' && buffer[6] == 'y' && buffer[7] == 'p')
            {
                // Move past the first 8 bytes to read the file type
                stream.Position = 8;
                var brandBuffer = new byte[4];
                var brandBytesRead = await stream.ReadAsync(brandBuffer, 0, 4, cancellationToken);
                
                if (brandBytesRead == 4)
                {
                    // Common video brands in ftyp box
                    // mp42: MP4 v2, avc1: H.264, isom: base MP4, qt: QuickTime
                    string brand = System.Text.Encoding.ASCII.GetString(brandBuffer);
                    string[] videoTypes = { "mp42", "avc1", "iso2", "iso6", "qt  ", "mmp4", "M4V " };
                    
                    foreach (var videoType in videoTypes)
                    {
                        if (brand == videoType)
                        {
                            return extension != ".m4a"; // Special case: M4A is audio-only
                        }
                    }
                    
                    // For MP4 containers, check for video tracks by scanning for 'mdat', 'moov', 'trak', 'vide'
                    // Reset position and look for these boxes
                    stream.Position = 0;
                    var searchBuffer = new byte[1024]; // Read chunks to search for video track identifiers
                    int searchBytesRead = await stream.ReadAsync(searchBuffer, 0, searchBuffer.Length, cancellationToken);
                    
                    if (searchBytesRead > 0)
                    {
                        string fileHeader = System.Text.Encoding.ASCII.GetString(searchBuffer, 0, searchBytesRead);
                        if (fileHeader.Contains("vide") || fileHeader.Contains("avc1") || fileHeader.Contains("hvc1"))
                        {
                            return true; // Contains video codec identifier
                        }
                    }
                }
            }

            // AVI format
            if (bytesRead >= 12 && buffer[0] == 'R' && buffer[1] == 'I' && buffer[2] == 'F' && buffer[3] == 'F' &&
                buffer[8] == 'A' && buffer[9] == 'V' && buffer[10] == 'I')
            {
                return true;
            }

            // WebM/MKV (EBML header)
            if (bytesRead >= 4 && buffer[0] == 0x1A && buffer[1] == 0x45 && buffer[2] == 0xDF && buffer[3] == 0xA3)
            {
                // Additional check for WebM/MKV signature
                return true;
            }

            // FLV format
            if (bytesRead >= 3 && buffer[0] == 'F' && buffer[1] == 'L' && buffer[2] == 'V')
            {
                return true;
            }
            
            // Some WMV files start with ASF header (same as WMA)
            if (bytesRead >= 4 && buffer[0] == 0x30 && buffer[1] == 0x26 && buffer[2] == 0xB2 && buffer[3] == 0x75)
            {
                // For WMV/WMA, check the file extension since they use the same container
                return extension == ".wmv";
            }

            // Check for 3GP format (which is based on MP4 but has different signatures)
            if (extension == ".3gp")
            {
                // 3GP files often have different ftyp brands
                if (bytesRead >= 8 && buffer[4] == 'f' && buffer[5] == 't' && buffer[6] == 'y' && buffer[7] == 'p')
                {
                    return true; // 3GP files with ftyp box are likely video
                }
                // Some 3GP files might have different headers, so default to true for .3gp extension
                return true;
            }

            return false;
        }
        catch
        {
            // If there's an error reading the file, assume it's not a video file
            return false;
        }
    }
}
