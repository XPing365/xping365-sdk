using System.Net;
using System.Net.Http.Headers;
using XPing365.Core.DataSource;
using XPing365.Shared;

namespace XPing365.Core.DataRetrieval
{
    public class WebRequestRetrieval : IWebDataRetrieval
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ServiceConfigurator configurator;

        public WebRequestRetrieval(
            IHttpClientFactory httpClientFactory, 
            ServiceConfigurator configurator)
        {
            this.httpClientFactory = httpClientFactory;
            this.configurator = configurator;
        }

        public async Task<T> GetFromHtmlAsync<T>(string url, TimeSpan timeout) where T : HtmlSource, new()
        {
            HttpClient httpClient = this.CreateHttpClient(timeout);

            DateTime requestStartTime = DateTime.UtcNow;
            DateTime requestEndTime = requestStartTime;
            string html = string.Empty;
            HttpStatusCode statusCode = 0;
            bool? isSuccessStatusCode = null;

            using (new InstrumentationLog((i) => requestEndTime = requestStartTime + i.ElapsedTime))
            {
                var response = await httpClient.GetAsync(url);
                html = await response.Content.ReadAsStringAsync();
                statusCode = response.StatusCode;
                isSuccessStatusCode = response.IsSuccessStatusCode;
            }

            T dataSource = new()
            {
                Url = GetAbsoluteRequestUrl(httpClient.BaseAddress, url),
                Html = html,
                RequestStartTime = requestStartTime,
                RequestEndTime = requestEndTime,
                ResponseCode = statusCode,
                IsSuccessResponseCode = isSuccessStatusCode.Value,
                ResponseSizeInBytes = html.Length * sizeof(char)
            };

            return dataSource; 
        }

        public static string GetAbsoluteRequestUrl(Uri? baseAddress, string url)
        {
            if (baseAddress != null)
            {
                return new Uri(baseAddress, url).ToString();
            }

            return url;
        }

        public HttpClient CreateHttpClient(TimeSpan timeout)
        {
            HttpClient httpClient;
            string? httpClientName = this.configurator.HttpRequestSection.HttpClientName;

            if (string.IsNullOrEmpty(httpClientName))
            {
                httpClient = this.httpClientFactory.CreateClient();
            }
            else
            {
                httpClient = this.httpClientFactory.CreateClient(httpClientName);
            }

            httpClient.Timeout = timeout;

            if (this.configurator.HttpRequestSection.Headers != null)
            {
                if (this.configurator.HttpRequestSection.Headers.ContainsKey("UserAgent"))
                {
                    var userAgent = this.configurator.HttpRequestSection.Headers["UserAgent"];
                    httpClient.DefaultRequestHeaders.UserAgent.Clear();
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(userAgent));
                }
            }

            return httpClient;
        }
    }
}
