using HtmlAgilityPack;

namespace XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;

internal interface ISelector
{
    public HtmlNodeCollection Select(HtmlNode node);
}
