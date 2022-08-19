using XPing365.Core.DataSource;

namespace XPing365.Core.DataParser
{
    public interface IDataParserFactory
    {
        IDataParser<T> Create<T>() where T : HtmlSource;
    }
}
