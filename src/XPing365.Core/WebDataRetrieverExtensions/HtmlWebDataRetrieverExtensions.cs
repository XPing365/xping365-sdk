using System.Net;
using XPing365.Core.Source;
using XPing365.Shared;

namespace XPing365.Core.WebDataRetrieverExtensions
{
    internal static class HtmlWebDataRetrieverExtensions
    {
        public static async Task<T> GetFromHtmlAsync<T>(this HttpClient httpClient, string url) where T : HtmlSource, new()
        {
            DateTime requestStartTime = DateTime.UtcNow;
            DateTime requestEndTime = requestStartTime;
            string html = string.Empty;
            HttpStatusCode? statusCode = null;
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
                Url = httpClient.GetRequestUrl(url),
                Html = html,
                RequestStartTime = requestStartTime,
                RequestEndTime = requestEndTime,
                ResponseCode = statusCode.Value,
                IsSuccessResponseCode = isSuccessStatusCode.Value,
                ResponseSizeInBytes = html.Length * sizeof(char)
            };

            return dataSource;
        }

        public static string GetRequestUrl(this HttpClient httpClient, string url)
        {
            if (httpClient.BaseAddress != null)
            {
                return new Uri(httpClient.BaseAddress, url).ToString();
            }

            return url;
        }
    }
}
