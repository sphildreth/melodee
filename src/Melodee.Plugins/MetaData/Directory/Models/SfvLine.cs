using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Plugins.MetaData.Directory.Models;

/// <summary>
/// Represents data found in a Sfv file line
/// </summary>
public sealed record SfvLine
{
    public bool IsValid { get; init; }

    public required FileSystemFileInfo FileSystemFileInfo { get; init; }


    public required string CrcHash { get; init; }

    public override string ToString()
    {
        return $"IsValid [{IsValid}] FileSystemFileInfo [{FileSystemFileInfo}]";
    }

    public bool Equals(SfvLine? other)
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
               string.Equals(FileSystemFileInfo.Name, other.FileSystemFileInfo.Name, StringComparison.OrdinalIgnoreCase) &&
               CrcHash == other.CrcHash;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsValid, FileSystemFileInfo, CrcHash);
    }
}