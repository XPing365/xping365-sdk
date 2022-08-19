using System.Collections.Specialized;
using Microsoft.Extensions.Configuration;

namespace XPing365.Core
{
    public class ServiceConfigurator
    {
        private readonly IConfiguration configuration;

        public class WebRequestRetrievalSettings
        {
            public string? HttpClientName { get; set; } = string.Empty;

            public bool ThrowOnError { get; set; } = false;

            public bool RetryOnFailure { get; set; } = true;

            public int RetryCount { get; set; } = 3;

            public int RetryDelayInSeconds { get; set; } = 3;

            public int TimeoutInSeconds { get; set; } = 10;

            public NameValueCollection? Headers { get; set; }

            public TimeSpan Timeout => TimeSpan.FromSeconds(this.TimeoutInSeconds);

            public TimeSpan RetryDelay => TimeSpan.FromSeconds(this.RetryDelayInSeconds);
        }

        public class DataParserSettings
        {
            public bool ThrowOnError { get; set; } = false;
        }

        public WebRequestRetrievalSettings WebRequestRetrievalSection => 
            this.configuration.GetSection(nameof(WebRequestRetrievalSettings)).Get<WebRequestRetrievalSettings>();

        public DataParserSettings DataParserSection => 
            this.configuration.GetSection(nameof(DataParserSettings)).Get<DataParserSettings>();

        public ServiceConfigurator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
