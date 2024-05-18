using System.Text.RegularExpressions;
using System.Xml.XPath;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Availability.Validations.Content.Html;

/// <summary>
/// Html locators represent a way to find element(s) on the html content.
/// </summary>
public interface IHtmlLocator
{
    /// <summary>
    /// This method narrows existing locator according to the options, for example filters by text. It can be chained 
    /// to filter multiple times.
    /// <code>
    /// html.GetByRole(AriaRole.Listitem)<br/>
    ///     .Filter(new() { HasText = "text in column 1" })<br/>
    ///     .Filter(new() {<br/>
    ///         Has = html.GetByRole(AriaRole.Button, new() { Name = "column 2 button" } )<br/>
    ///     })<br/>
    ///     .Visible();
    /// </code>
    /// </summary>
    /// <param name="options">Call options</param>
    IHtmlLocator Filter(FilterOptions options);

    /// <summary>
    /// Returns locator to the first matching element.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).First().HasInnerText("Some text");</code>
    /// </summary>
    IHtmlLocator First();

    /// <summary>
    /// Returns locator to the last matching element.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).Last().HasInnerText("Some text");</code>
    /// </summary>
    IHtmlLocator Last();

    /// <summary>
    /// Returns locator to the n-th matching element. It's zero based, <c>nth(0)</c> selects the first element.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).Nth(2).HasInnerText("Some text");</code>
    /// </summary>
    /// <param name="index">
    /// Zero based index of the matching element. 
    /// </param>
    /// <exception cref="ValidationException">
    /// When the index is outside the allowable range of matching elements. The exception is reported as failure in
    /// the <see cref="TestSession"/>.
    /// </exception>
    IHtmlLocator Nth(int index);

    /// <summary>
    /// The method finds an element matching the specified selector in the locator's subtree.
    /// It also accepts filter options, similar to <see cref="Filter"/> method.
    /// </summary>
    /// <param name="selector">A selector or locator to use when resolving DOM element.</param>
    /// <param name="options">Optional parameter for customizing the locator behavior.</param>
    IHtmlLocator Locator(XPathExpression selector, FilterOptions? options = default);

    /// <summary>
    /// Validates that the specified number matches the number of elements corresponding to the provided locator.
    /// Usage: <code>html.GetByRole(AriaRole.Listitem).HasCount(expectedCount);</code>
    /// where 'expectedCount' is the number of elements expected to be found.
    /// </summary>
    /// <param name="count">The expected number of elements to match the locator.</param>
    void HasCount(int count);

    /// <summary>
    /// Confirms that the innerText of the element identified by the locator matches the specified string.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).HasInnerText("Sample Text");</code>
    /// In this example, "Sample Text" is the string anticipated to match the innerText of the located element.
    /// </summary>
    /// <param name="innerText">The string to verify against the innerText of the located element.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    void HasInnerText(string innerText, TextOptions? options = null);

    /// <summary>
    /// Confirms that the innerText of the element identified by the locator matches the specified regex.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).HasInnerText("Sample Text");</code>
    /// In this example, "Sample Text" is the string anticipated to match the innerText of the located element.
    /// </summary>
    /// <param name="innerText">The string to verify against the innerText of the located element.</param>
    void HasInnerText(Regex innerText);

    /// <summary>
    /// Confirms that the innerHtml of the element identified by the locator matches the specified string.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).HasInnerHtml("&lt;div&gt;");</code>
    /// In this example, "&lt;div&gt;" is the string anticipated to match the innerHtml of the located element.
    /// </summary>
    /// <param name="innerHtml">The string to verify against the innerHtml of the located element.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    void HasInnerHtml(string innerHtml, TextOptions? options = null);

    /// <summary>
    /// Confirms that the innerHtml of the element identified by the locator matches the specified regex.
    /// Example: <code>html.GetByRole(AriaRole.Listitem).HasInnerText("&lt;div&gt;");</code>
    /// In this example, "&lt;div&gt;" is the string anticipated to match the innerHtml of the located element.
    /// </summary>
    /// <param name="innerHtml">The string to verify against the innerHtml of the located element.</param>
    void HasInnerHtml(Regex innerHtml);
}
