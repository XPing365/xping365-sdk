using XPing365.Core.DataSource;

namespace XPing365.Core.DataParser
{
    public interface IDataParser<T> where T : HtmlSource
    {
        T Parse(ref T data);
    }
}
