namespace Melodee.Plugins.MetaData.Release.Models;

[Serializable]
public sealed record M3ULine
{
    public bool IsValid { get; init; }
    
    public required FileSystemInfo FileInfo { get; init; }
    
    public int TrackNumber { get; init; }
    
    public string? ReleaseArist { get; init; }
    
    public required string TrackTitle { get; init; }
    
    public override string ToString()
    {
        return $"IsValid [{IsValid}] ReleaseArtist [{ReleaseArist}] TrackNumber [{TrackNumber}] TrackTitle [{TrackTitle}]";
    }
    public bool Equals(M3ULine? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (ReferenceEquals(this, null))
        {
            return false;
        }
        if (ReferenceEquals(other, null))
        {
            return false;
        }
        if (this.GetType() != other.GetType())
        {
            return false;
        }

        return IsValid == other.IsValid &&
               string.Equals(FileInfo.FullName, other.FileInfo.FullName, StringComparison.OrdinalIgnoreCase) &&
               TrackNumber == other.TrackNumber &&
               ReleaseArist == other.ReleaseArist &&
               TrackTitle == other.TrackTitle;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsValid, FileInfo, TrackNumber, ReleaseArist, TrackTitle);
    }    
}