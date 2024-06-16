using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Plugins.Discovery;

namespace Melodee.Plugins.MetaData;

public sealed class SimpleFileVerification
{

    public bool HandlesFile(string fullFilename)
    {
        var ext = Path.GetExtension(fullFilename);
        if (!FileHelper.IsFileMediaMetaDataType(ext))
        {
            return false;
        }
        return string.Equals(ext.Replace(".", ""), "sfv");
    }


    public Task<OperationResult<Release>> ProcessFile(string file, CancellationToken cancellationToken = default)
    {
        
        
        throw new NotImplementedException();
    }
    
    private static async Task<int> GetMp3CountFromSfvFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return 0;
        }
        var result = 0;
        foreach (var line in await File.ReadAllLinesAsync(filePath))
        {
            if (IsLineForFileForTrack(line))
            {
                result++;
            }
        }
        return result;
    }    
    
    private static string? Mp3FileNameFromSfvLine(string? lineFromFile)
    {
        if (string.IsNullOrWhiteSpace(lineFromFile))
        {
            return null;
        }
        var parts = lineFromFile.Split(' ');
        return parts.Take(parts.Length - 1).ToCsv(" ");
    }    
    
    private static bool IsLineForFileForTrack(string? lineFromFile)
    {
        if (lineFromFile?.Nullify() == null)
        {
            return false;
        }
        if (lineFromFile.StartsWith("#") || lineFromFile.StartsWith(";"))
        {
            return false;
        }
        // TODO this should use the FileHelper versus magic string extensions 
        if (lineFromFile.Contains(".mp3", StringComparison.OrdinalIgnoreCase) ||
            lineFromFile.Contains(".flac", StringComparison.OrdinalIgnoreCase) ||
            lineFromFile.Contains(".wav", StringComparison.OrdinalIgnoreCase) ||
            lineFromFile.Contains(".ac4", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }    
}