using System.Net.Http.Headers;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Availability.Validators;

/// <summary>
/// The HttpResponseHeadersValidator class is a concrete implementation of the <see cref="TestComponent"/> class that 
/// is used to validate the response headers of an HTTP response. It takes a Func&lt;HttpResponseHeaders, bool&gt; 
/// delegate as a parameter, which is used to validate the response headers. The errorMessage parameter is an optional 
/// error message that can be used to provide additional information about the validation failure.
/// </summary>
/// <example>
/// <code>
/// var responseHeadersValidator = new HttpResponseHeadersValidator(
///    isValid: (HttpResponseHeaders headers) => headers.Contains(HeaderNames.UserAgent),
///    errorMessage: (HttpResponseHeaders headers) => 
///        $"The HTTP response headers did not include the expected $'{HeaderNames.UserAgent}' header."
/// );
/// var validator = new ValidationPipeline(responseHeadersValidator);
/// </code>
/// </example>
/// <param name="isValid">Func&lt;HttpResponseHeaders, bool&gt; delegate to validate the response headers.</param>
/// <param name="errorMessage">Optional information about the validation failure.</param>
public class HttpResponseHeadersValidator(
    Func<HttpResponseHeaders, bool> isValid,
    Func<HttpResponseHeaders, string>? errorMessage = null) : TestComponent(StepName, TestStepType.ValidateStep)
{
    public const string StepName = "Http response headers validation";

    private readonly Func<HttpResponseHeaders, bool> _isValid = isValid;
    private readonly Func<HttpResponseHeaders, string>? _errorMessage = errorMessage;

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        using var instrumentation = new InstrumentationLog();
        TestStep testStep = null!;

        try
        {
            HttpResponseHeaders responseHeaders =
                context.SessionBuilder
                    .PropertyBag.GetProperty<HttpResponseHeaders>(PropertyBagKeys.HttpResponseHeaders);

            // Perform test step validation.
            bool isValid = _isValid(responseHeaders);

            if (isValid)
            {
                testStep = context.SessionBuilder.Build(component: this, instrumentation);
            }
            else
            {
                string? errmsg = _errorMessage?.Invoke(responseHeaders);
                testStep = context.SessionBuilder.Build(
                    component: this,
                    instrumentation: instrumentation,
                    error: Errors.ValidationFailed(component: this, errmsg));
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
