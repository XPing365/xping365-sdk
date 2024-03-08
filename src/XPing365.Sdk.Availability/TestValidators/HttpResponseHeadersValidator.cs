using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Extensions;
using System.Net.Http.Headers;

namespace XPing365.Sdk.Availability.TestValidators;

/// <summary>
/// The HttpResponseHeadersValidator class is a concrete implementation of the <see cref="TestComponent"/> class that 
/// is used to validate the response headers of an HTTP response. It takes a Func&lt;HttpResponseHeaders, bool&gt; 
/// delegate as a parameter, which is used to validate the response headers. The onError parameter is an optional 
/// error message that can be used to provide additional information about the validation failure.
/// </summary>
/// <remarks>
/// <note>
/// The HttpResponseHeadersValidator component requires the HttpRequestSender component to be registered before it in 
/// the pipeline, because it depends on the HTTP response results from the HttpRequestSender component.
/// </note>
/// <example>
/// <code>
/// var responseHeadersValidator = new HttpResponseHeadersValidator(
///    isValid: (HttpResponseHeaders headers) => headers.Contains(HeaderNames.UserAgent),
///    onError: (HttpResponseHeaders headers) => 
///        $"The HTTP response headers did not include the expected $'{HeaderNames.UserAgent}' header."
/// );
/// var validator = new ValidationPipeline(responseHeadersValidator);
/// </code>
/// </example>
/// </remarks>
/// <param name="isValid">Func&lt;HttpResponseHeaders, bool&gt; delegate to validate the response headers.</param>
/// <param name="onError">Optional information about the validation failure.</param>
public class HttpResponseHeadersValidator(
    Func<HttpResponseHeaders, bool> isValid,
    Func<HttpResponseHeaders, string>? onError = null) : TestComponent(StepName, TestStepType.ValidateStep)
{
    /// <summary>
    /// The name of the test component that represents a HttpResponseHeadersValidator test operation.
    /// </summary>
    /// <remarks>
    /// This constant is used to register the HttpResponseHeadersValidator class in the test framework.
    /// </remarks>
    public const string StepName = "Http response headers validation";

    private readonly Func<HttpResponseHeaders, bool> _isValid = isValid;
    private readonly Func<HttpResponseHeaders, string>? _onError = onError;

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">
    /// An optional CancellationToken object that can be used to cancel the this operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// If any of the following parameters: url, settings or context is null.
    /// </exception>
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
            var responseMessage = context.GetNonSerializablePropertyBagValue<HttpResponseMessage>(
                PropertyBagKeys.HttpResponseMessage);

            if (responseMessage == null)
            {
                testStep = context.SessionBuilder.Build(
                    component: this,
                    instrumentation: instrumentation,
                    error: Errors.InsufficientData(component: this));
            }
            else
            {
                // Perform test step validation.
                bool isValid = _isValid(responseMessage.Headers);

                if (isValid)
                {
                    testStep = context.SessionBuilder.Build(component: this, instrumentation);
                }
                else
                {
                    string? errmsg = _onError?.Invoke(responseMessage.Headers);
                    testStep = context.SessionBuilder.Build(
                        component: this,
                        instrumentation: instrumentation,
                        error: Errors.ValidationFailed(component: this, errmsg));
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

        return Task.CompletedTask;
    }
}
