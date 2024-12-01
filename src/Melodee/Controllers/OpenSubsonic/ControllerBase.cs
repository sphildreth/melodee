using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.Scrobbling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Melodee.Controllers.OpenSubsonic;

public abstract class ControllerBase : Controller
{
    protected ApiRequest ApiRequest { get; private set; } = null!;

    private static T? GetHeaderValueAs<T>(HttpContext? context, string headerName)
    {
        if (context?.Request.Headers.TryGetValue(headerName, out var values) ?? false)
        {
            var rawValues = values.ToString();
            if (!string.IsNullOrWhiteSpace(rawValues))
            {
                return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
        }

        return default;
    }

    private static string GetRequestIp(HttpContext? context, bool tryUseXForwardHeader = true)
    {
        string? ip = null;
        if (tryUseXForwardHeader)
        {
            ip = GetHeaderValueAs<string>(context, "X-Forwarded-For").FromDelimitedList(delimiter: ',')?.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(ip) && context?.Connection.RemoteIpAddress != null)
        {
            ip = context.Connection.RemoteIpAddress.ToString();
        }

        if (string.IsNullOrWhiteSpace(ip))
        {
            ip = GetHeaderValueAs<string>(context, "REMOTE_ADDR");
        }

        if (string.IsNullOrWhiteSpace(ip))
        {
            throw new Exception("Unable to determine caller's IP.");
        }

        return ip;
    }

    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var values = new List<KeyValue>();
        foreach (var header in context.HttpContext.Request.Headers)
        {
            values.Add(new KeyValue(header.Key, header.Value.ToString()));
        }

        if (context.HttpContext.Request.HasFormContentType)
        {
            values.Add(new KeyValue("u", context.HttpContext.Request.Form["u"]));
            values.Add(new KeyValue("v", context.HttpContext.Request.Form["v"]));
            values.Add(new KeyValue("f", context.HttpContext.Request.Form["f"]));
            values.Add(new KeyValue("apiKey", context.HttpContext.Request.Form["apiKey"]));
            values.Add(new KeyValue("p", context.HttpContext.Request.Form["p"]));
            values.Add(new KeyValue("t", context.HttpContext.Request.Form["t"]));
            values.Add(new KeyValue("s", context.HttpContext.Request.Form["s"]));
            values.Add(new KeyValue("c", context.HttpContext.Request.Form["c"]));
        }
        else
        {
            values.Add(new KeyValue("u", context.HttpContext.Request.Query["u"].FirstOrDefault()));
            values.Add(new KeyValue("v", context.HttpContext.Request.Query["v"].FirstOrDefault()));
            values.Add(new KeyValue("f", context.HttpContext.Request.Query["f"].FirstOrDefault()));
            values.Add(new KeyValue("apiKey", context.HttpContext.Request.Query["apiKey"].FirstOrDefault()));
            values.Add(new KeyValue("p", context.HttpContext.Request.Query["p"].FirstOrDefault()));
            values.Add(new KeyValue("t", context.HttpContext.Request.Query["t"].FirstOrDefault()));
            values.Add(new KeyValue("s", context.HttpContext.Request.Query["s"].FirstOrDefault()));
            values.Add(new KeyValue("c", context.HttpContext.Request.Query["c"].FirstOrDefault()));
        }

        ApiRequest = new ApiRequest
        (
            values.ToArray(),
            values.FirstOrDefault(x => x.Key == "u")?.Value,
            values.FirstOrDefault(x => x.Key == "v")?.Value,
            values.FirstOrDefault(x => x.Key == "f")?.Value,
            values.FirstOrDefault(x => x.Key == "apiKey")?.Value,
            values.FirstOrDefault(x => x.Key == "p")?.Value,
            values.FirstOrDefault(x => x.Key == "t")?.Value,
            values.FirstOrDefault(x => x.Key == "s")?.Value,
            new UserPlayer
            (
                values.FirstOrDefault(x => x.Key == "User-Agent")?.Value,
                values.FirstOrDefault(x => x.Key == "c")?.Value,
                values.FirstOrDefault(x => x.Key == "Host")?.Value,
                GetRequestIp(context.HttpContext)
            )
        );
        string? xmlWarning = null;
        if (!ApiRequest.IsJsonRequest)
        {
            xmlWarning = "\u2620\ufe0f WARNING: client requested non supported XML format response.";
        }
        Console.WriteLine($"-*->{ xmlWarning } User [{ApiRequest.Username}] : {ApiRequest.ApiRequestPlayer}");
        return base.OnActionExecutionAsync(context, next);
    }
}
