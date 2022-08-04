using XPing365.Core.Source;

namespace XPing365.Core.Parser
{
    public interface IParserFactory
    {
        IParser<T> Create<T>() where T : HtmlSource;
    }
}
