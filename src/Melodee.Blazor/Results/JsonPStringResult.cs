using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Results;

/// <summary>
/// https://www.rfc-editor.org/rfc/rfc4329.txt
/// </summary>
public sealed class JsonPStringResult : ContentResult
{
    public JsonPStringResult(string json)
    {
        Content = json;
        ContentType = "application/javascript; charset=utf-8";
    }
}
