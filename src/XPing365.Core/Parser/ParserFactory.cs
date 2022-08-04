using XPing365.Core.Parser.Internals;
using XPing365.Core.Source;

namespace XPing365.Core.Parser
{
    public class ParserFactory : IParserFactory
    {
        public IParser<T> Create<T>() where T : HtmlSource
        {
            return new DefaultParser<T>();
        }
    }
}
