using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;
using Melodee.Common.Metadata.AudioTags.Readers;

namespace Melodee.Common.Metadata.AudioTags;

public static class AudioTagManager
{
    public static async Task<IEnumerable<FileInfo>> AllMediaFilesForDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        if (!directoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
        }

        // Using FileFormatDetector.DetectFormatAsync return all files in the directory where the detected format is not null
        var files = new List<FileInfo>();
        var allFiles = directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
        foreach (var file in allFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var format = await FileFormatDetector.DetectFormatAsync(file.FullName, cancellationToken);
                if (format != AudioFormat.Unknown)
                {
                    files.Add(file);
                }
            }
            catch
            {
                // Ignore files that cannot be read or do not match any known format
                // This is useful for skipping unsupported or corrupted files
            }
        }

        return files;
    }

    public static async Task<AudioTagData> ReadAllTagsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var format = await FileFormatDetector.DetectFormatAsync(filePath, cancellationToken);
        ITagReader? reader = format switch
        {
            AudioFormat.Mp3 => new Id3TagReader(),
            AudioFormat.Ape => new ApeTagReader(),
            AudioFormat.Mp4 => new Mp4TagReader(),
            AudioFormat.Wma => new WmaTagReader(),
            AudioFormat.Vorbis => new VorbisTagReader(),
            _ => null
        };
        if (reader == null)
        {
            throw new NotSupportedException($"Unsupported or unknown audio format for file: {filePath}");
        }

        var tags = await reader.ReadTagsAsync(filePath, cancellationToken);
        var images = await reader.ReadImagesAsync(filePath, cancellationToken);
        var metadata = await FileMetadataReader.GetFileMetadataAsync(filePath);
        return new AudioTagData
        {
            Format = format,
            Tags = new Dictionary<MetaTagIdentifier, object>(tags),
            Images = images,
            FileMetadata = metadata
        };
    }
}
