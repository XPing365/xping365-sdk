using Microsoft.Playwright;
using XPing365.Sdk.Availability.TestActions;
using XPing365.Sdk.Availability.TestActions.Configurations;
using XPing365.Sdk.Availability.Validations.Content.Html;
using XPing365.Sdk.Availability.Validations.Content.Page;
using XPing365.Sdk.Availability.Validations.HttpResponse;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Clients.Configurations;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Extensions;

/// <summary>
/// Provides extension methods for the TestAgent class to enhance its functionality.
/// </summary>
public static class TestAgentExtensions
{
    /// <summary>
    /// Configures the TestAgent to perform DNS lookups.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    public static TestAgent UseDnsLookup(this TestAgent testAgent)
    {
        return testAgent.RequireNotNull(nameof(testAgent)).Replace(new DnsLookup());
    }

    /// <summary>
    /// Configures the TestAgent to check IP address accessibility using ping.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    /// <param name="options">Optional configuration settings for the ping operation.</param>
    public static TestAgent UseIPAddressAccessibilityCheck(
        this TestAgent testAgent, Action<PingConfiguration>? options = null)
    {
        var configuration = new PingConfiguration();
        options?.Invoke(configuration);

        return testAgent.RequireNotNull(nameof(testAgent)).Replace(new IPAddressAccessibilityCheck(configuration));
    }

    /// <summary>
    /// Configures the TestAgent to send requests using an HttpClient.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    /// <param name="options">Optional configuration settings for the HttpClient.</param>
    public static TestAgent UseHttpClient(this TestAgent testAgent, Action<HttpClientConfiguration>? options = null)
    {
        var configuration = new HttpClientConfiguration();
        options?.Invoke(configuration);

        return testAgent.RequireNotNull(nameof(testAgent)).Replace(new HttpClientRequestSender(configuration));
    }

    /// <summary>
    /// Configures the TestAgent to simulate browser requests.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    /// <param name="options">Optional configuration settings for the browser simulation.</param>
    public static TestAgent UseBrowserClient(this TestAgent testAgent, Action<BrowserConfiguration>? options = null)
    {
        var configuration = new BrowserConfiguration();
        options?.Invoke(configuration);

        return testAgent.RequireNotNull(nameof(testAgent)).Replace(new BrowserRequestSender(configuration));
    }

    /// <summary>
    /// Configures the TestAgent with a http response validator.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    /// <param name="response">The validation logic to use on the HTTP response.</param>
    /// <returns></returns>
    public static TestAgent UseHttpValidation(this TestAgent testAgent, Action<IHttpResponse> response)
    {
        return testAgent.RequireNotNull(nameof(testAgent)).Add(new HttpResponseValidator(response));
    }

    /// <summary>
    /// Configures the TestAgent with a page content validator.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    /// <param name="page">The validation logic to use on the Page object.</param>
    public static TestAgent UsePageValidation(this TestAgent testAgent, Action<IPage> page)
    {
        return testAgent.RequireNotNull(nameof(testAgent)).Add(new PageContentValidator(page));
    }

    /// <summary>
    /// Configures the TestAgent with a html content validator.
    /// </summary>
    /// <param name="testAgent">The instance of TestAgent to configure.</param>
    /// <param name="htmlContent">The validation logic to use on the Html content.</param>
    public static TestAgent UseHtmlValidation(this TestAgent testAgent, Action<IHtmlContent> htmlContent)
    {
        return testAgent.RequireNotNull(nameof(testAgent)).Add(new HtmlContentValidator(htmlContent));
    }
}
