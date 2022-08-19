using System.Net;
using XPing365.Core.DataSource;
using XPing365.Shared;

namespace XPing365.Core.DataRetrieval
{
    internal class WebRequestRetrieval : IWebDataRetrieval
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
            HttpClient httpClient = this.CreateHttpClient();

            DateTime requestStartTime = DateTime.UtcNow;
            DateTime requestEndTime = requestStartTime;
            string html = string.Empty;
            HttpStatusCode? statusCode = null;
            bool? isSuccessStatusCode = null;
            httpClient.Timeout = timeout;

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
                ResponseCode = statusCode.Value,
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

        private HttpClient CreateHttpClient()
        {
            HttpClient httpClient;
            string? httpClientName = this.configurator.WebRequestRetrievalSection.HttpClientName;

            if (string.IsNullOrEmpty(httpClientName))
            {
                httpClient = this.httpClientFactory.CreateClient();
            }
            else
            {
                httpClient = this.httpClientFactory.CreateClient(httpClientName);
            }

            return httpClient;
        }
    }
}
