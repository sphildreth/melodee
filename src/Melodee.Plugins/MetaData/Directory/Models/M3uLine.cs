using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Directory.Models;

public sealed record M3ULine
{
    public bool IsValid { get; init; }

    public required FileSystemFileInfo FileSystemFileInfo { get; init; }

    public int SongNumber { get; init; }

    public string? AlbumArist { get; init; }

    public required string SongTitle { get; init; }

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
               SongNumber == other.SongNumber &&
               AlbumArist == other.AlbumArist &&
               SongTitle == other.SongTitle;
    }

    public override string ToString()
    {
        return $"IsValid [{IsValid}] AlbumArtist [{AlbumArist}] SongNumber [{SongNumber}] SongTitle [{SongTitle}]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsValid, FileSystemFileInfo, SongNumber, AlbumArist, SongTitle);
    }
}
