using System.Xml.XPath;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals;

internal class XPath(string name, XPathExpression expression)
{
    public string Name { get; } = name.RequireNotNullOrEmpty(nameof(name));
    public XPathExpression Expression { get; } = expression.RequireNotNull(nameof(expression));
}

internal static class XPaths
{
    private static XPath? _testId;
    private static readonly Lazy<XPath> _alt = new(() => 
        new XPath("alt", XPathExpression.Compile($"//*[@alt]")));
    private static readonly Lazy<XPath> _title = new(() => 
        new XPath("page-title", XPathExpression.Compile($"//head/title")));
    private static readonly Lazy<XPath> _titleAttr = new(() =>
        new XPath("title", XPathExpression.Compile($"//*[@title]")));
    private static readonly Lazy<XPath> _label = new(() => 
        new XPath("label", XPathExpression.Compile($"//label")));
    private static readonly Lazy<XPath> _placeholder = new(() => 
        new XPath("placeholder", XPathExpression.Compile($"//*[@placeholder]")));

    public static XPath Alt => _alt.Value;
    public static XPath Title => _title.Value;
    public static XPath TitleAttribute => _titleAttr.Value;
    public static XPath Label => _label.Value;
    public static XPath Placeholder => _placeholder.Value;
    public static XPath TestIdAttribute(string testIdAttribute)
    {
        ArgumentException.ThrowIfNullOrEmpty(testIdAttribute);

        if (_testId == null || _testId.Name != testIdAttribute)
        {
            _testId = new XPath(testIdAttribute, XPathExpression.Compile($"//*[@{testIdAttribute}]"));
        }

        return _testId;
    }
}
