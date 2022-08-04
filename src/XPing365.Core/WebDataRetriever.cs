using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XPing365.Core.Parser;
using XPing365.Core.Parameter;
using XPing365.Core.Source;
using XPing365.Core.WebDataRetrieverExtensions;
using XPing365.Shared;

namespace XPing365.Core
{
    public class WebDataRetriever : IWebDataRetriever
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IParserFactory dataParserFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<WebDataRetriever> logger;

        public WebDataRetriever(IHttpClientFactory httpClientFactory, IParserFactory dataParserFactory, IConfiguration configuration, ILogger<WebDataRetriever> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.dataParserFactory = dataParserFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<T?> GetFromHtmlAsync<T>(string url) where T : HtmlSource, new()
        {
            try
            {
                HttpClient httpClient = this.CreateHttpClient();
                T html = await httpClient.GetFromHtmlAsync<T>(url.RequireNotNullOrWhiteSpace(nameof(url)));
                IParser<T> dataParser = this.dataParserFactory.Create<T>();
                T parsedData = dataParser.Parse(ref html);
                
                return parsedData;
            }
            catch (Exception ex)
            {
                this.logger?.LogError("WebDataRetriever encountered an issue: {Message}", ex.Message);
                this.ThrowOnErrorIfConfigured(ex);
            }

            return null;
        }

        public async IAsyncEnumerable<T?> GetFromHtmlAsync<T>(string url, IParameterSet parameterSet) where T : HtmlSource, new()
        {
            foreach (var query in parameterSet.CreateBuilder(url).Build())
            {
                T? parsedData = null;
                try
                {
                    HttpClient httpClient = this.CreateHttpClient();
                    T html = await httpClient.GetFromHtmlAsync<T>(query);
                    IParser<T> dataParser = this.dataParserFactory.Create<T>();
                    parsedData = dataParser.Parse(ref html);
                }
                catch (Exception ex)
                {
                    this.logger?.LogError("WebDataRetriever encountered an issue: {Message}", ex.Message);
                    this.ThrowOnErrorIfConfigured(ex);
                }

                yield return parsedData;
            }
        }

        private HttpClient CreateHttpClient()
        {
            HttpClient httpClient;
            string httpClientName = this.configuration["WebDataRetriever:HttpClientName"];

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

        private void ThrowOnErrorIfConfigured(Exception ex)
        {
            if (bool.TryParse(this.configuration["WebDataRetriever:ThrowOnError"], out bool throwOnError) && throwOnError)
            {
                // Bubble up exception 
                throw ex;
            }
        }
    }
}