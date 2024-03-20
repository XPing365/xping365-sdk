using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Session.Comparison.Internals.MarkdownDecorators;

internal abstract class BaseMdDecorator(ITextReport textReport) : ITextReport
{
    private readonly ITextReport _textReport = textReport.RequireNotNull(nameof(textReport));

    public virtual string Generate()
    {
        return _textReport.Generate();
    }
}
