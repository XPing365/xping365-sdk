using XPing365.Core.DataSource;

namespace XPing365.Core.DataRetrieval
{
    public class WebBrowserRetrieval : IWebDataRetrieval
    {
        public Task<T> GetFromHtmlAsync<T>(string url, TimeSpan timeout) where T : HtmlSource, new()
        {
            throw new NotImplementedException();
        }
    }
}
