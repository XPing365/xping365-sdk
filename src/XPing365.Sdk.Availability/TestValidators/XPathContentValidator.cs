using System.Xml.XPath;
using HtmlAgilityPack;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestValidators;

/// <summary>
/// Represents a validator that checks if an HTML document or a node is valid according to a specified XPath expression.
/// </summary>
public class XPathContentValidator : BaseContentValidator
{
    public const string StepName = "Regex content validator";

    private readonly XPathExpression _xpath;
    private readonly Func<HtmlNodeCollection, bool> IsValid;
    private readonly string? _onError;

    /// <summary>
    /// Initializes a new instance of the <see cref="XPathContentValidator"/> class.
    /// </summary>
    /// <param name="xpath">An xpath expression to use for validation.</param>
    /// <param name="isValid">A function that determines whether the node collection is valid or not.</param>
    /// <param name="onError">An optional error message to display when the validation fails.</param>
    /// <exception cref="ArgumentNullException">Thrown when xpath or isValid function is null.</exception>
    /// <example>
    /// <code>
    /// var xpath = XPathExpression.Compile("//ul[@class='navbar-nav']/li/a[@href='/Identity/Account/Login']/text()");
    /// 
    /// var component = new XPathContentValidator(
    ///     xpath: xpath,
    ///     isValid: (nodes) => nodes.First().InnerText == "Login",
    ///     onError: $"The HTML document does not match the XPath expression: '{xpath.Expression}'");
    /// </code>
    /// </example>
    public XPathContentValidator(
        XPathExpression xpath,
        Func<HtmlNodeCollection, bool> isValid,
        string? onError = null) : base(StepName)
    {
        _xpath = xpath.RequireNotNull(nameof(xpath));
        IsValid = isValid.RequireNotNull(nameof(isValid));
        _onError = onError;
    }

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">
    /// An optional CancellationToken object that can be used to cancel this operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If any of the following parameters: url, settings or context is null.
    /// </exception>
    /// <remarks>
    /// <note type="tip">
    /// This method uses HtmlAgilityPack to validate the XPath expression against the HTML document and it does not 
    /// report an error when the document is not a well-formed XML document.
    /// </note>
    /// </remarks>
    public override Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        using var instrumentation = new InstrumentationLog();
        TestStep testStep = null!;

        try
        {
            var response = context.GetNonSerializablePropertyBagValue<HttpResponseMessage>(
                PropertyBagKeys.HttpResponseMessage);
            var data = context.GetPropertyBagValue<byte[]>(PropertyBagKeys.HttpContent);

            if (response == null || data == null)
            {
                testStep = context.SessionBuilder.Build(
                    component: this,
                    instrumentation: instrumentation,
                    error: Errors.InsufficientData(component: this));
            }
            else
            {
                string content = GetContent(data, response.Content.Headers);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(content);
                HtmlNode root = htmlDocument.DocumentNode;

                // Perform test step validation.
                bool isValid = IsValid(root.SelectNodes(_xpath));

                if (isValid)
                {
                    testStep = context.SessionBuilder.Build(component: this, instrumentation);
                }
                else
                {
                    testStep = context.SessionBuilder.Build(
                        component: this,
                        instrumentation: instrumentation,
                        error: Errors.ValidationFailed(component: this, _onError));
                }
            }
        }
        catch (Exception exception)
        {
            testStep = context.SessionBuilder.Build(component: this, instrumentation, exception);
        }
        finally
        {
            context.Progress?.Report(testStep);
        }

        return Task.FromResult(testStep);
    }
}
