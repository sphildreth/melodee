using Microsoft.AspNetCore.Mvc;

namespace Melodee.Results;

public sealed class XmlStringResult : ContentResult
{
    public XmlStringResult(string xml)
    {
        Content = xml;
        //headers.Add("Content-Type", "text/xml");
        ContentType = "application/xml";
    }
}
