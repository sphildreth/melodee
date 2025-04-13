using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record InternetRadioStation(string Id, string Name, string StreamUrl, string? HomePageUrl) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        return $"<internetRadioStation id=\"{Id}\" name=\"{Name.ToSafeXmlString()}\" streamUrl=\"{StreamUrl.ToSafeXmlString()}\" homePageUrl=\"{HomePageUrl.ToSafeXmlString()}\" />";
    }    
}
