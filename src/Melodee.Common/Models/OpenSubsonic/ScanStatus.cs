using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public sealed record ScanStatus(bool Scanning, int Count) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        return $"<scanStatus scanning=\"{Scanning.ToLowerCaseString()}\" count=\"{Count}\"/>";
    }
}
