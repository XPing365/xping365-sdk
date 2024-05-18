using System.Xml.XPath;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals;

internal static class XPathExpressions
{
    private readonly static Dictionary<string, Lazy<XPathExpression>> _xpathsMap = new()
    {
        { "ALT", new(() => XPathExpression.Compile($"//*[@alt]")) },
        { "TITLE", new(() => XPathExpression.Compile($"//head/title")) },
        { "LABEL", new(() => XPathExpression.Compile($"//label")) }
    };

    public static XPathExpression XPath(string name) => _xpathsMap[name.ToUpperInvariant()].Value;

    public static XPathExpression Alt => _xpathsMap["ALT"].Value;
    public static XPathExpression Title => _xpathsMap["TITLE"].Value;
    public static XPathExpression Label => _xpathsMap["LABEL"].Value;
}
