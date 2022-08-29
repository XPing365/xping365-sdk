using System.Xml;
using XPing365.Shared;

namespace XPing365.Core.DataSource.Internal
{
    internal class XPathDefinitionWithXmlConfig : HtmlSource
    {
        public string? XmlConfig { get; set; }

        public string? Json { get; set; }

        public XmlDocument GetXmlConfig()
        {
            XmlDocument doc = new();
            doc.LoadXml(this.XmlConfig.RequireNotNull(nameof(this.XmlConfig)));

            return doc;
        }
    }
}
