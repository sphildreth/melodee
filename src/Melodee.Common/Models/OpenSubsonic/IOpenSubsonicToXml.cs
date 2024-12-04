namespace Melodee.Common.Models.OpenSubsonic;

public interface IOpenSubsonicToXml
{
    string ToXml(string? nodeName = null);
}
