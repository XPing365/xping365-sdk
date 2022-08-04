using XPing365.Core.Source;

namespace XPing365.Core.Parser
{
    public interface IParser<T> where T : HtmlSource
    {
        T Parse(ref T data);
    }
}
