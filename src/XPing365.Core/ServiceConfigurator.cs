using Microsoft.Extensions.Configuration;

namespace XPing365.Core
{
    public class ServiceConfigurator
    {
        private readonly IConfiguration configuration;

        public class HttpRequestSettings
        {
            public string? HttpClientName { get; set; } = string.Empty;

            public bool RetryOnFailure { get; set; } = true;

            public int RetryCount { get; set; } = 3;

            public int RetryDelayInSeconds { get; set; } = 3;

            public int TimeoutInSeconds { get; set; } = 10;

            public Dictionary<string, string>? Headers { get; set; }

            public TimeSpan Timeout => TimeSpan.FromSeconds(this.TimeoutInSeconds);

            public TimeSpan RetryDelay => TimeSpan.FromSeconds(this.RetryDelayInSeconds);
        }

        public class WebBrowserSettings
        {
            public string Path { get; set; } = string.Empty;
        }

        public class DataParserSettings
        {
            public bool ThrowOnError { get; set; } = false;
        }

        public bool ThrowOnError => this.configuration.GetValue<bool>("ThrowOnError");

        public HttpRequestSettings HttpRequestSection => 
            this.configuration.GetSection(nameof(HttpRequestSettings)).Get<HttpRequestSettings>();

        public WebBrowserSettings WebBrowserSection =>
            this.configuration.GetSection(nameof(WebBrowserSettings)).Get<WebBrowserSettings>();

        public DataParserSettings DataParserSection => 
            this.configuration.GetSection(nameof(DataParserSettings)).Get<DataParserSettings>();

        public ServiceConfigurator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
