using System.Text.RegularExpressions;
using System.Xml.XPath;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability.Validations.Content.Html;

/// <summary>
/// Defines a contract for HTML content manipulation and element location within a web page.
/// </summary>
public interface IHtmlContent
{
    /// <summary>
    /// Validates that an HTML document or element has the specified title.
    /// The title is typically found within the &lt;title&gt; tag in the &lt;head&gt; section of an HTML document.
    /// </summary>
    /// <param name="title">The expected title to validate against.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    void HasTitle(string title, TextOptions? options = null);

    /// <summary>
    /// Validates that the size in bytes of an HTML document is equal to or less than the specified maximum size.
    /// </summary>
    /// <param name="maxSizeInBytes">The maximum allowed size of the HTML document in bytes.</param>
    void HasMaxDocumentSize(int maxSizeInBytes);

    /// <summary>
    /// Locates an HTML element using an XPath selector and returns a locator for further actions.
    /// </summary>
    /// <param name="selector">The XPath expression used to resolve the DOM element.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    /// <returns>An IHtmlLocator instance representing the located element.</returns>
    IHtmlLocator Locator(XPathExpression selector, FilterOptions? options = default);

    /// <summary>
    /// Locates elements with an 'alt' attribute text matching the specified string.
    /// </summary>
    /// <param name="text">The text to match against the 'alt' attribute.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByAltText(string text, TextOptions? options = null);

    /// <summary>
    /// Locates elements with an 'alt' attribute text matching the specified regular expression.
    /// </summary>
    /// <param name="text">The regular expression to match against the 'alt' attribute.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByAltText(Regex text);

    /// <summary>
    /// Locates elements with a 'label' element text matching the specified string.
    /// </summary>
    /// <param name="text">The text to match against the 'label' element text.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByLabel(string text, TextOptions? options = null);

    /// <summary>
    /// Locates elements with an 'label' element text matching the specified regular expression.
    /// </summary>
    /// <param name="text">The regular expression to match against the 'label' element text.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByLabel(Regex text);

    /// <summary>
    /// Locates elements with a 'placeholder' attribute text matching the specified string.
    /// </summary>
    /// <param name="text">The text to match against the 'placeholder' attribute.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByPlaceholder(string text, TextOptions? options = null);

    /// <summary>
    /// Locates elements with a 'placeholder' attribute text matching the specified regular expression.
    /// </summary>
    /// <param name="text">The regular expression to match against the 'placeholder' attribute.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByPlaceholder(Regex text);

    /// <summary>
    /// Locates elements with a test id attribute matching the specified string. By default, the `data-testid` attribute 
    /// is used as a test id. Use <see cref="TestSettings.TestIdAttribute"/> to configure a different test id attribute 
    /// if necessary.
    /// </summary>
    /// <param name="testId">The text to match against the test id attribute.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByTestId(string testId, TextOptions? options = null);

    /// <summary>
    /// Locates elements with a test id attribute matching the specified regular expression. By default, the 
    /// `data-testid` attribute is used as a test id. Use <see cref="TestSettings.TestIdAttribute"/> to configure a 
    /// different test id attribute if necessary.
    /// </summary>
    /// <param name="testId">The regular expression to match against the test id attribute.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByTestId(Regex testId);

    /// <summary>
    /// Locates elements with a 'title' attribute text matching the specified string.
    /// </summary>
    /// <param name="text">The text to match against the 'title' attribute.</param>
    /// <param name="options">Optional parameters for customizing the locator behavior.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByTitle(string text, TextOptions? options = null);

    /// <summary>
    /// Locates elements with a 'title' attribute text matching the specified regular expression.
    /// </summary>
    /// <param name="text">The regular expression to match against the 'title' attribute.</param>
    /// <returns>An IHtmlLocator instance representing the located elements.</returns>
    IHtmlLocator GetByTitle(Regex text);
}
