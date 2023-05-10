using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrderHelperLib.Utl
{

    public static class Http
    {
        private static HttpClient HttpClient { get; } = new HttpClient();

        public static async Task<(bool isSuccess, string content)> SendRequestAsync(string baseUrl, HttpMethod method,
            object content = null, string accessToken = null, params (string, string)[] queryParams)
        {
            var query = queryParams is { Length: > 0 }
                ? "?" + string.Join("&",
                    queryParams.Select(qp => $"{Uri.EscapeDataString(qp.Item1)}={Uri.EscapeDataString(qp.Item2)}"))
                : string.Empty;
            var url = $"{baseUrl}{query}";

            using var request = new HttpRequestMessage(method, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            if (content != null)
            {
                var stringContent = JsonConvert.SerializeObject(content);
                request.Content = new StringContent(stringContent, Encoding.UTF8, "application/json");
            }

            var client = HttpClient;
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return (false, $"Error code = {response.StatusCode}!");
            var responseContent = await response.Content.ReadAsStringAsync();
            return (true, responseContent);
        }

        public static Task<HttpResponseMessage> PostAsync(string url, string content)
        {
            var http = new HttpClient();
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return http.PostAsync(url, stringContent);
        }

        public static Task<HttpResponseMessage> GetAsync(string url)
        {
            var http = new HttpClient();
            return http.GetAsync(url);
        }

        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, string content)
        {
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return client.PostAsync(url, stringContent);
        }

        public static HttpClient InstanceAccessClient(string accessToken) => new AccessHttpClient(accessToken);
        public static HttpClient InstanceRefreshClient(string refreshToken) => new RefreshTokenClient(refreshToken);

        private class AccessHttpClient : HttpClient
        {
            private string _accessToken;

            public string Access_Token => _accessToken;

            public AccessHttpClient(string accessToken)
            {
                _accessToken = accessToken;
                DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }

        private class RefreshTokenClient : HttpClient
        {
            private string _refreshToken;

            public string RefreshToken => _refreshToken;

            public RefreshTokenClient(string refreshToken)
            {
                _refreshToken = refreshToken;
                DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _refreshToken);
            }
        }
    }
}