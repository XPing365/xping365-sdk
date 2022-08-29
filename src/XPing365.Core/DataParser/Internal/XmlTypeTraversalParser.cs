using System.Text;
using System.Xml;
using HtmlAgilityPack;
using XPing365.Core.DataParser.Converters;
using XPing365.Core.DataSource.Internal;
using XPing365.Shared;

namespace XPing365.Core.DataParser.Internal
{
    internal class XmlTypeTraversalParser : IDataParser<XPathDefinitionWithXmlConfig>
    {
        private static readonly Lazy<IValueConverter> defaultConverter = 
            new(() => new DefaultValueConverter());

        public XPathDefinitionWithXmlConfig Parse(ref XPathDefinitionWithXmlConfig dataSource)
        {
            HtmlNode rootNode = dataSource.GetHtmlDocument().DocumentNode;

            XmlDocument document = dataSource.GetXmlConfig();

            StringBuilder jsonOutput = new("{");
            Traverse(rootNode, document.FirstChild, ref jsonOutput);

            dataSource.Json = jsonOutput.Append('}').ToString();

            return dataSource;
        }

        private void Traverse(HtmlNode rootNode, XmlNode? xpathDef, ref StringBuilder jsonBuilder, bool insideArray = false)
        {
            if (rootNode == null || xpathDef == null || jsonBuilder == null)
            {
                return;
            }
            
            if (xpathDef != null && xpathDef.HasChildNodes)
            {
                foreach (XmlNode childNode in xpathDef.ChildNodes)
                {
                    string? xpath = childNode.Attributes?["XPath"]?.Value;
                    string? xpathAttr = childNode.Attributes?["attribute"]?.Value;
                    string type = (childNode.Attributes?["type"]?.Value).RequireNotNull("type");
                    Enum.TryParse(childNode.Attributes?["return-type"]?.Value, out ReturnType returnType);
                    Type? t = Type.GetType(type);

                    if (t == null)
                    {
                        throw new ArgumentException($"Incorrect type value in XML configuration. Could not create type from '{type}'");
                    }

                    Type targetType = t;
                    XPathAttribute attribute = xpathAttr == null ? 
                        new XPathAttribute(xpath, returnType) :
                        new XPathAttribute(xpath, xpathAttr);

                    string fieldName = childNode.Name.Trim();

                    if (xpath == null)
                    {
                        if (targetType.IsClass)
                        {
                            var startTag = insideArray ? "{" : '"' + fieldName + '"' + ":{";
                            jsonBuilder.Append(startTag);
                            this.Traverse(rootNode, childNode, ref jsonBuilder);
                            jsonBuilder.Append("},");
                        }
                    }
                    else
                    {
                        try
                        {
                            if (targetType.IsList())
                            {
                                jsonBuilder.Append('"' + fieldName + '"' + ":[");
                                var selectedNodes = rootNode.SelectNodes(xpath); 

                                foreach (var selectedNode in selectedNodes)
                                {
                                    jsonBuilder.Append('{');
                                    this.Traverse(selectedNode, childNode, ref jsonBuilder, insideArray: true);
                                    jsonBuilder.Append("},");
                                }
                                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                                jsonBuilder.Append("],");
                                continue;
                            }
                            if (targetType.IsClass && targetType != typeof(string))
                            {
                                var startTag = insideArray ? "{" : '"' + fieldName + '"' + ":{";
                                jsonBuilder.Append(startTag);
                                var singleNode = rootNode.SelectSingleNode(xpath);
                                this.Traverse(singleNode, childNode, ref jsonBuilder);
                                jsonBuilder.Append("},");
                                continue;
                            }
                            else
                            {
                                HtmlNode singleNode = rootNode.SelectSingleNode(xpath);
                                string value = singleNode.GetValue(attribute);

                                jsonBuilder.Append("\"" + fieldName + "\":");

                                if (!string.IsNullOrEmpty(value))
                                {
                                    var result = defaultConverter.Value.Convert(value, targetType);

                                    if (result is string strResult)
                                    {
                                        jsonBuilder.Append("\"" + strResult + "\",");
                                    }
                                    else
                                    {
                                        if (result != null)
                                        {
                                            jsonBuilder.Append(result?.ToString() + ",");
                                        }
                                        else
                                        {
                                            jsonBuilder.Append($"\"{value}\",");
                                        }
                                    }
                                }
                                else
                                {
                                    jsonBuilder.Append("\"\",");
                                }
                            }
                        }
                        catch (NodeNotFoundException)
                        {
                        }
                    }
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            }
        }
    }
}
