using XPing365.Core.DataSource;

namespace XPing365.Core.DataRetrieval
{
    public interface IWebDataRetrieval
    {
        Task<T> GetFromHtmlAsync<T>(string url, TimeSpan timeout) where T : HtmlSource, new();
    }
}
