using XPing365.Core.Parameter;
using XPing365.Core.Source;

namespace XPing365.Core
{
    public interface IWebDataRetriever
    {
        Task<T?> GetFromHtmlAsync<T>(string url) where T : HtmlSource, new();

        IAsyncEnumerable<T?> GetFromHtmlAsync<T>(string url, IParameterSet parameterSet) where T : HtmlSource, new();
    }
}
