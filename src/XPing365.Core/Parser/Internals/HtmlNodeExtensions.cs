using HtmlAgilityPack;
using XPing365.Shared;

namespace XPing365.Core.Parser.Internals
{
    internal static class HtmlNodeExtensions
    {
        public static string GetNodeAttributeValue(this HtmlNode node, XPathAttribute attribute)
        {
            attribute.RequireNotNull(nameof(attribute));

            return node?.Attributes?.FirstOrDefault(a => a.Name == attribute.AttributeName)?.Value.Trim() ?? string.Empty;
        }

        public static string GetNodeValue(this HtmlNode node, ReturnType returnType)
        {
            if (node == null)
            {
                return string.Empty;
            }

            return returnType switch
            {
                ReturnType.InnerHtml => node.InnerHtml.Trim(),
                ReturnType.InnerText => node.InnerText.Trim(),
                ReturnType.OuterHtml => node.OuterHtml.Trim(),
                _ => node.InnerText.Trim(),
            };
        }

        public static string GetValue(this HtmlNode node, XPathAttribute attribute)
        {
            if (node == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(attribute.AttributeName))
            {
                return node.GetNodeAttributeValue(attribute);
            }

            return node.GetNodeValue(attribute.NodeReturnType);
        }
    }
}
