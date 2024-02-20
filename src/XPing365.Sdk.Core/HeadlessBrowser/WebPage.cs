namespace XPing365.Sdk.Core.HeadlessBrowser;

/// <summary>
/// This class represents a web page response that is obtained from a headless browser, such as Chromium, Firefox, or 
/// WebKit. It uses the HttpResponseMessage class to store the HTTP response message, including the status code, 
/// headers, and content.
/// </summary>
/// <remarks>
/// Its constructor initializes a new instance of the WebPage class with the specified <see cref="HttpResponseMessage"/> 
/// object that represents the HTTP response message from the headless browser.
/// 
/// Actual results are then transfered to the <see cref="Session.TestSession"/> objects to its
/// PropertyBag. 
/// </remarks>
public class WebPage
{
    private readonly HttpResponseMessage _responseMessage;

    public WebPage(HttpResponseMessage responseMessage)
    {
        _responseMessage = responseMessage;
    }

    /// <summary>
    /// A read-only property that gets the <see cref="HttpResponseMessage"/> object that is associated with the web 
    /// page.
    /// </summary>
    public HttpResponseMessage HttpResponseMessage => _responseMessage;
}
