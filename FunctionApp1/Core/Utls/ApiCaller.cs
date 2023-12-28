using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;
using WebUtlLib;

namespace Q_DoApi.Core.Utls;

public class ApiCaller
{
    private const string JsonMediaType = "application/json";

    private static Uri GetUri(string url, string controller, string action, params (string, object)[] queries)
    {
        var builder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(builder.Query);
        builder.Path = builder.Path.TrimEnd('/') + $"api/{controller}/{action}";
        foreach (var (q, value) in queries)
            query[q] = value.ToString();
        builder.Query = query.ToString();
        return builder.Uri;
    }

    private static HttpContent GetContent(string stringContent) =>
        new StringContent(stringContent, Encoding.UTF8, JsonMediaType);

    private static async Task<string> ResponseProceed(HttpResponseMessage response, [CallerMemberName] string methodName = null)
    {
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"{nameof(ApiCaller)}.{methodName}:Unable to get response from [{response.RequestMessage?.RequestUri}]! with code[({(int)response.StatusCode}){response.StatusCode}]");
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> ApiGet(string url, string controller, string action, ILogger log,
        params (string, object)[] queries)
    {
        var uri = GetUri(url, controller, action, queries);
        log.Event($"Get({action}):{uri}");
        using var client = new HttpClient();
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
        return await ResponseProceed(response);
    }

    public static async Task<string> ApiPost(string url, string controller, string action, string stringContent, ILogger log,
        params (string, object)[] queries)
    {
        var uri = GetUri(url, controller, action, queries);
        log.Event($"Post({action}):{uri}\n{stringContent}");
        var content = GetContent(stringContent);
        using var client = new HttpClient();
        var response = await client.PostAsync(uri, content);
        return await ResponseProceed(response);
    }

}