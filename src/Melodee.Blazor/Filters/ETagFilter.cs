using Melodee.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Melodee.Blazor.Filters;

public class ETagFilter : IResourceFilter
{
    private const int OkStatusCode = 200;
    private const int NotModifiedStatusCode = 304;
    private const string IdKey = "id";

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        if (context.HttpContext.Request.Method == "GET")
        {
            if (context.HttpContext.Response.StatusCode == OkStatusCode)
            {
                if (context.HttpContext.Request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
                {
                    var incomingEtag = context.HttpContext.Request.Headers[HeaderNames.IfNoneMatch].ToString();
                    string? apiKey;
                    if (context.HttpContext.Request.HttpContext.Request.HasFormContentType)
                    {
                        apiKey = context.HttpContext.Request.HttpContext.Request.Form[IdKey];
                    }
                    else
                    {
                        apiKey = context.HttpContext.Request.HttpContext.Request.Query[IdKey].FirstOrDefault();
                    }

                    if (string.IsNullOrEmpty(apiKey))
                    {
                        apiKey = context.HttpContext.Request.HttpContext.Request.RouteValues
                                     .FirstOrDefault(x => x.Key.ToNormalizedString() == IdKey.ToNormalizedString())
                                     .Value?.ToString() ??
                                 string.Empty;
                    }

                    var etagRepository = context.HttpContext.RequestServices.GetRequiredService<EtagRepository>();
                    if (etagRepository.EtagMatch(apiKey, incomingEtag))
                    {
                        context.Result = new StatusCodeResult(NotModifiedStatusCode);
                    }
                }
            }
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
