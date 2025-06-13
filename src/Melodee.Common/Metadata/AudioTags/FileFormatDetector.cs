namespace Melodee.Common.Metadata.AudioTags;

public static class FileFormatDetector
{
    public static async Task<AudioFormat> DetectFormatAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        if (bytesRead < 4)
        {
            return AudioFormat.Unknown;
        }

        // MP3 ID3v2
        if (buffer[0] == 'I' && buffer[1] == 'D' && buffer[2] == '3')
        {
            return AudioFormat.MP3;
        }

        // MP3 frame sync bits (0xFF followed by 0xFB, 0xFA, 0xF3, etc.)
        if (buffer[0] == 0xFF && (buffer[1] & 0xE0) == 0xE0)
        {
            return AudioFormat.MP3;
        }

        // MP3 ID3v1 (check last 128 bytes for "TAG")
        if (stream.Length >= 128)
        {
            var id3v1Buffer = new byte[3];
            stream.Seek(-128, SeekOrigin.End);
            var id3v1Read = await stream.ReadAsync(id3v1Buffer, 0, 3, cancellationToken);
            if (id3v1Read == 3 && id3v1Buffer[0] == 'T' && id3v1Buffer[1] == 'A' && id3v1Buffer[2] == 'G')
            {
                return AudioFormat.MP3;
            }
        }

        // MP4 (ftyp)
        if (buffer[4] == 'f' && buffer[5] == 't' && buffer[6] == 'y' && buffer[7] == 'p')
        {
            return AudioFormat.MP4;
        }

        // WMA (ASF header)
        if (buffer[0] == 0x30 && buffer[1] == 0x26 && buffer[2] == 0xB2 && buffer[3] == 0x75)
        {
            return AudioFormat.WMA;
        }

        // Ogg Vorbis
        if (buffer[0] == 'O' && buffer[1] == 'g' && buffer[2] == 'g' && buffer[3] == 'S')
        {
            return AudioFormat.Vorbis;
        }
        
        // FLAC detection
        if (buffer[0] == 'f' && buffer[1] == 'L' && buffer[2] == 'a' && buffer[3] == 'C')
        {
            return AudioFormat.Vorbis; // FLAC uses Vorbis comments, so map to Vorbis format
        }

        // APE (Monkey's Audio)
        if (buffer[0] == 'M' && buffer[1] == 'A' && buffer[2] == 'C' && buffer[3] == ' ')
        {
            return AudioFormat.APE;
        }
        
        // If we get here and the file extension is .flac, assume it's a FLAC file
        // This handles cases where the file might not have the standard header
        if (Path.GetExtension(filePath).Equals(".flac", StringComparison.OrdinalIgnoreCase))
        {
            return AudioFormat.Vorbis;
        }
        
        // If we get here and the file extension is .ogg, assume it's an Ogg Vorbis file
        if (Path.GetExtension(filePath).Equals(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            return AudioFormat.Vorbis;
        }

        return AudioFormat.Unknown;
    }
}
