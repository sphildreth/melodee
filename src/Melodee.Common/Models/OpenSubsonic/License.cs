using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record License(bool Valid, string Email, string LicenseExpires, string TrailExpires) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        var licenseExpiresAttribute = string.Empty;
        if (LicenseExpires.Nullify() != null)
        {
            licenseExpiresAttribute = $" licenseExpires=\"{LicenseExpires}\"";
        }

        var trailExpiresAttribute = string.Empty;
        if (TrailExpires.Nullify() != null)
        {
            trailExpiresAttribute = $" trialExpires=\"{LicenseExpires}\"";
        }

        return
            $"<license valid=\"{Valid.ToLowerCaseString()}\" email=\"{Email.ToSafeXmlString()}\"{licenseExpiresAttribute} {trailExpiresAttribute} />";
    }
}
