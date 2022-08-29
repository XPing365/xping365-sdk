using Microsoft.Extensions.Logging;
using XPing365.Core.DataParser;
using XPing365.Core.Parameter;
using XPing365.Core.DataSource;
using XPing365.Shared;
using XPing365.Core.DataRetrieval;
using XPing365.Core.DataSource.Internal;

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
                        return this.webDataRetrieval.GetFromHtmlAsync<T>(url, configurator.HttpRequestSection.Timeout);
                    }, 
                    this.configurator.HttpRequestSection.RetryDelay, 
                    this.configurator.HttpRequestSection.RetryOnFailure ?
                        this.configurator.HttpRequestSection.RetryCount : 1);

                IDataParser<T> dataParser = this.dataParserFactory.Create(html);
                T parsedData = dataParser.Parse(ref html);
                
                return parsedData;
            }
            catch (Exception ex)
            {
                this.logger?.LogError(nameof(WebDataCapture) + " encountered an issue: {Message}", ex.Message);
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

        public async Task<string?> GetFromHtmlAsync(string url, string xmlConfig)
        {
            try
            {
                XPathDefinitionWithXmlConfig html = await Retry.DoAsync(
                    () => {
                        return this.webDataRetrieval.GetFromHtmlAsync<XPathDefinitionWithXmlConfig>(url, configurator.HttpRequestSection.Timeout);
                    },
                    this.configurator.HttpRequestSection.RetryDelay,
                    this.configurator.HttpRequestSection.RetryOnFailure ?
                        this.configurator.HttpRequestSection.RetryCount : 1);
                
                html.XmlConfig = xmlConfig.RequireNotNullOrWhiteSpace(nameof(xmlConfig));
                
                IDataParser<XPathDefinitionWithXmlConfig> dataParser = this.dataParserFactory.Create(html);
                XPathDefinitionWithXmlConfig parsedData = dataParser.Parse(ref html);

                return parsedData.Json;
            }
            catch (Exception ex)
            {
                this.logger?.LogError(nameof(WebDataCapture) + " encountered an issue: {Message}", ex.Message);
                this.ThrowOnErrorIfConfigured(ex);
            }

            return null;
        }

        private void ThrowOnErrorIfConfigured(Exception ex)
         {
            if (this.configurator.ThrowOnError)
            {
                // Bubble up exception 
                throw ex;
            }
        }
    }
}