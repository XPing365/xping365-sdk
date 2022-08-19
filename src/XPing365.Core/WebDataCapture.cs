using Microsoft.Extensions.Logging;
using XPing365.Core.DataParser;
using XPing365.Core.Parameter;
using XPing365.Core.DataSource;
using XPing365.Shared;
using XPing365.Core.DataRetrieval;

namespace XPing365.Core
{
    public class WebDataCapture
    {
        private readonly IWebDataRetrieval webDataRetrieval;
        private readonly IDataParserFactory dataParserFactory;
        private readonly ILogger<WebDataCapture> logger;
        private readonly ServiceConfigurator configurator;

        public WebDataCapture(
            IWebDataRetrieval webDataRetrieval,
            IDataParserFactory dataParserFactory, 
            ILogger<WebDataCapture> logger, 
            ServiceConfigurator configuration)
        {
            this.webDataRetrieval = webDataRetrieval;
            this.dataParserFactory = dataParserFactory;
            this.configurator = configuration;
            this.logger = logger;
        }

        public async Task<T?> GetFromHtmlAsync<T>(string url) where T : HtmlSource, new()
        {
            try
            {
                T html = await Retry.DoAsync(
                    () => {
                        return this.webDataRetrieval.GetFromHtmlAsync<T>(url, configurator.WebRequestRetrievalSection.Timeout);
                    }, 
                    this.configurator.WebRequestRetrievalSection.RetryDelay, 
                    this.configurator.WebRequestRetrievalSection.RetryOnFailure ?
                        this.configurator.WebRequestRetrievalSection.RetryCount : 1);

                IDataParser<T> dataParser = this.dataParserFactory.Create<T>();
                T parsedData = dataParser.Parse(ref html);
                
                return parsedData;
            }
            catch (Exception ex)
            {
                this.logger?.LogError("WebDataCapture encountered an issue: {Message}", ex.Message);
                this.ThrowOnErrorIfConfigured(ex);
            }

            return null;
        }

        public async IAsyncEnumerable<T?> GetFromHtmlAsync<T>(string url, IParameterSet parameterSet) where T : HtmlSource, new()
        {
            foreach (var query in parameterSet.CreateBuilder(url).Build())
            {
                T? parsedData = await this.GetFromHtmlAsync<T>(query);
                yield return parsedData;
            }
        }

        private void ThrowOnErrorIfConfigured(Exception ex)
        {
            if (this.configurator.WebRequestRetrievalSection.ThrowOnError)
            {
                // Bubble up exception 
                throw ex;
            }
        }
    }
}