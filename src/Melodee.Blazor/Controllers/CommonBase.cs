using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers;

public abstract class CommonBase : Controller
{
    protected ApiRequest ApiRequest { get; set; } = null!;

    protected string GetBaseUrl(IConfiguration configuration)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
        var configuredDomain = configuration.GetSection("Melodee:BaseUrl").Value.Nullify();
        return configuredDomain ?? baseUrl;
    }
    
    protected static string GetRequestIp(HttpContext? context, bool tryUseXForwardHeader = true)
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
    
    protected static T? GetHeaderValueAs<T>(HttpContext? context, string headerName)
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
}
