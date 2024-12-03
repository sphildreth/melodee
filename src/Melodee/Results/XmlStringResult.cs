using Microsoft.AspNetCore.Mvc;

namespace Melodee.Results;

public sealed class XmlStringResult : ContentResult
{
    public XmlStringResult(string xml)
    {
        Content = xml;
        ContentType = "application/xml; charset=utf-8";
    }
}
