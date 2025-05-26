using System.Net;
using Microsoft.AspNetCore.Http;

namespace Melodee.Common.Extensions;

public static class HttpContextExtensions
{
    public static bool IsImageRequest(this HttpContext context)
    {
        return context.Request.Path.Value != null && context.Request.Path.Value.Contains("/images/", StringComparison.OrdinalIgnoreCase);
    }
    
    public static bool IsLocalRequest(this HttpContext context)
    {
        if (context.Connection.RemoteIpAddress == null)
            return false;
            
        if (context.Connection.LocalIpAddress == null)
            return context.Connection.RemoteIpAddress.Equals(IPAddress.Loopback) || 
                   context.Connection.RemoteIpAddress.Equals(IPAddress.IPv6Loopback);
            
        return context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress) || 
               context.Connection.RemoteIpAddress.Equals(IPAddress.Loopback) || 
               context.Connection.RemoteIpAddress.Equals(IPAddress.IPv6Loopback);
    }
}
