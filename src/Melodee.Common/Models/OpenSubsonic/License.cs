using System.Text;

namespace Melodee.Common.Models.OpenSubsonic;

public record License(bool Valid, string Email, string LicenseExpires, string TrailExpires) : IOpenSubsonicToXml
{
    public string ToXml()
    {
        return $"<license valid=\"{Valid}\" email=\"{Email}\" licenseExpires=\"{ LicenseExpires }\"/>";
    }
}
