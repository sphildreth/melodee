using Melodee.Blazor.Filters;
using Melodee.Blazor.Results;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Serialization;
using Melodee.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public abstract class ControllerBase(EtagRepository etagRepository, ISerializer serializer) : Controller
{
    protected readonly ISerializer Serializer = serializer;

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

    protected async Task<IActionResult> ImageResult(string apiKey, Task<ResponseModel> action)
    {
        try
        {
            var model = await action;
            if (model.ResponseData.Etag == null || model.ResponseData.Data is not byte[])
            {
                Log.Warning("ResponseData is invalid for ApiKey [{ApiKey}]", apiKey);
                return new EmptyResult();
            }
            HttpContext.Response.Headers.Append("ETag", model.ResponseData.Etag);
            etagRepository.AddEtag(model.ApiKeyId, model.ResponseData.Etag);
            return new FileContentResult((byte[])model.ResponseData.Data!, model.ResponseData.ContentType ?? "image/jpeg");
        }
        catch (OperationCanceledException)
        {
            // Don't do anything as this happens a lot with TCP connections
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error in image result for ApiKey [{ApiKey}]", apiKey);
        }
        return new EmptyResult();
    }

    protected async Task<IActionResult> MakeResult(Task<ResponseModel> modelTask)
    {
        var modelData = await modelTask.ConfigureAwait(false);

        Response.Headers.Append("X-Content-Type-Options", "nosniff");
        Response.Headers.Append("X-Frame-Options", "DENY");
        Response.Headers.Append("X-Total-Count", modelData.TotalCount.ToString());
        Response.Headers.Append("Referrer-Policy", "same-origin");
        Response.Headers.Append("Vary", "Origin");

        if (ApiRequest.IsJsonRequest)
        {
            return new JsonStringResult(Serializer.Serialize(modelData)!);
        }

        if (ApiRequest.IsJsonPRequest)
        {
            return new JsonPStringResult($"{ApiRequest.Callback}({Serializer.Serialize(modelData)})");
        }

        return new XmlStringResult(Serializer.SerializeOpenSubsonicModelToXml(modelData)!);
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
            values.Add(new KeyValue("callback", context.HttpContext.Request.Form["callback"]));
            values.Add(new KeyValue("jwt", context.HttpContext.Request.Form["jwt"]));
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
            values.Add(new KeyValue("callback", context.HttpContext.Request.Query["callback"].FirstOrDefault()));
            values.Add(new KeyValue("jwt", context.HttpContext.Request.Query["jwt"].FirstOrDefault()));
        }
        values.Add(new KeyValue("QueryString", context.HttpContext.Request.QueryString.ToString()));
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
            values.FirstOrDefault(x => x.Key == "callback")?.Value,
            values.FirstOrDefault(x => x.Key == "jwt")?.Value,
            new UserPlayer
            (
                values.FirstOrDefault(x => x.Key == "User-Agent")?.Value,
                values.FirstOrDefault(x => x.Key == "c")?.Value,
                values.FirstOrDefault(x => x.Key == "Host")?.Value,
                GetRequestIp(context.HttpContext)
            )
        );
        Console.WriteLine($"-*-> User [{ApiRequest.Username}] : { Serializer.Serialize(ApiRequest)}");
        return base.OnActionExecutionAsync(context, next);
    }
}