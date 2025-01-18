namespace Melodee.Tests;

/// <summary>
/// Test wrapper to actually perform (not mocked) a HTTP request
/// </summary>
/// <param name="httpClient"></param>
public class TestHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => httpClient;
}
