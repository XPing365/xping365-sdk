namespace XPing365.Sdk.Core.Session.Comparison.Internals.MarkdownDecorators;

internal class BoldTextMdDecorator(ITextReport textReport) : BaseMdDecorator(textReport)
{
    private const string BoldMarker = "**";

    public override string Generate()
    {
        return BoldMarker + base.Generate() + BoldMarker;
    }
}
