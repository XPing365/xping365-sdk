using XPing365.Core.DataParser.Internal;
using XPing365.Core.DataSource;

namespace XPing365.Core.DataParser
{
    public class DataParserFactory : IDataParserFactory
    {
        public IDataParser<T> Create<T>() where T : HtmlSource
        {
            return new DefaultDataParser<T>();
        }
    }
}
