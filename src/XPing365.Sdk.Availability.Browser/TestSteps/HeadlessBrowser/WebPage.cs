namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser;

public class WebPage
{
    private readonly HttpResponseMessage _responseMessage;

    public WebPage(HttpResponseMessage responseMessage)
    {
        _responseMessage = responseMessage;
    }

    public HttpResponseMessage HttpResponseMessage => _responseMessage;
}
