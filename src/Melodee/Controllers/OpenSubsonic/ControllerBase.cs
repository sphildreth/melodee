using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
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
            string rawValues = values.ToString();
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
        ApiRequest = new ApiRequest
        (
            context.HttpContext.Request.Query["u"].FirstOrDefault(),
            context.HttpContext.Request.Query["v"].FirstOrDefault(),
            context.HttpContext.Request.Query["f"].FirstOrDefault(),
            context.HttpContext.Request.Query["id"].FirstOrDefault(),
            context.HttpContext.Request.Query["p"].FirstOrDefault(),
            context.HttpContext.Request.Query["t"].FirstOrDefault(),
            context.HttpContext.Request.Query["s"].FirstOrDefault(),
            new ApiRequestPlayer
            (
                context.HttpContext.Request.Headers["User-Agent"].ToString(),
                context.HttpContext.Request.Query["c"].FirstOrDefault(),
                context.HttpContext.Request.Headers["Host"].ToString(),
                GetRequestIp(context.HttpContext)
            )
        );
        return base.OnActionExecutionAsync(context, next);
    }
}
