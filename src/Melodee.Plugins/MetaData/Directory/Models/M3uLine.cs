using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Directory.Models;

[Serializable]
public sealed record M3ULine
{
    public bool IsValid { get; init; }

    public required FileSystemFileInfo FileSystemFileInfo { get; init; }

    public int TrackNumber { get; init; }

    public string? ReleaseArist { get; init; }

    public required string TrackTitle { get; init; }

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

        if (GetType() != other.GetType())
        {
            return false;
        }

        return IsValid == other.IsValid &&
               string.Equals(FileSystemFileInfo.Name, other.FileSystemFileInfo.Name, StringComparison.OrdinalIgnoreCase) &&
               TrackNumber == other.TrackNumber &&
               ReleaseArist == other.ReleaseArist &&
               TrackTitle == other.TrackTitle;
    }

    public override string ToString()
    {
        return $"IsValid [{IsValid}] ReleaseArtist [{ReleaseArist}] TrackNumber [{TrackNumber}] TrackTitle [{TrackTitle}]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsValid, FileSystemFileInfo, TrackNumber, ReleaseArist, TrackTitle);
    }
}
