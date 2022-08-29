using XPing365.Core.DataParser.Internal;
using XPing365.Core.DataSource;
using XPing365.Core.DataSource.Internal;

namespace XPing365.Core.DataParser
{
    public class DataParserFactory : IDataParserFactory
    {
        public IDataParser<T> Create<T>(T dataSource) where T : HtmlSource
        {
            return dataSource switch
            {
                XPathDefinitionWithXmlConfig => (IDataParser<T>)new XmlTypeTraversalParser(),
                _ => new ClassTypeTraversalParser<T>(),
            };
        }
    }
}
