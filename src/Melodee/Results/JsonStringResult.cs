using Microsoft.AspNetCore.Mvc;

namespace Melodee.Results;

public sealed class JsonStringResult : ContentResult
{
    public JsonStringResult(string json)
    {
        Content = json;
        ContentType = "application/json; charset=utf-8";
    }
}
