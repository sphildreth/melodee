using System.Diagnostics;
using System.Net;
using HtmlAgilityPack;

namespace Melodee.Services.Extensions;

public static class HttpClientFactoryExtensions
{
    public static async Task<byte[]?> BytesForImageUrlAsync(this IHttpClientFactory httpclientFactory, string userAgent, string? url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }
        try
        {
            var client = httpclientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", userAgent);
            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (WebException wex)
        {
            var err = "";
            try
            {
                if (wex.Response != null)
                {
                    using (var sr = new StreamReader(wex.Response.GetResponseStream()))
                    {
                        err = await sr.ReadToEndAsync(cancellationToken);
                    }
                }

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(err);
                err = (htmlDoc.DocumentNode.InnerText ?? string.Empty).Trim();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error getting image url [{url}], error: {ex}");
            }

            throw new Exception(err);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error with url [{url}] Exception [{ex}]", "Warning");
        }

        return null;
    }
}
