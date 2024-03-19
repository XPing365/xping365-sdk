namespace XPing365.Sdk.Core.Session.Comparison.Internals.MarkdownDecorators;

internal class CodeTextMdDecorator(ITextReport textReport) : BaseMdDecorator(textReport)
{
    private const char CodeMarker = '`';

    public override string Generate()
    {
        return CodeMarker + base.Generate() + CodeMarker;
    }
}
