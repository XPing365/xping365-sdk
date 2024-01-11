using System.Net.Http.Headers;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validators;

public class HttpResponseHeadersValidator(
    Func<HttpResponseHeaders, bool> isValidFunc,
    string? errorMessage = null) : TestStepHandler(StepName, TestStepType.ValidateStep)
{
    public const string StepName = "Http response headers validation";

    private readonly Func<HttpResponseHeaders, bool> _isValidFunc = isValidFunc;
    private readonly string? _errorMessage = errorMessage;

    public override Task<TestStep> HandleStepAsync(
        Uri uri,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        TestStep? sendHttpRequestStep = session.Steps.FirstOrDefault(step => step.Name == SendHttpRequest.StepName);

        if (sendHttpRequestStep == null)
        {
            return Task.FromResult(CreateFailedTestStep(Errors.InsufficientData(handler: this)));
        }

        HttpResponseHeaders responseHeaders = 
            sendHttpRequestStep.PropertyBag.GetProperty<HttpResponseHeaders>(
                PropertyBagKeys.HttpResponseHeaders);
        HttpResponseHeaders responseTrailingHeaders =
            sendHttpRequestStep.PropertyBag.GetProperty<HttpResponseHeaders>(
                PropertyBagKeys.HttpResponseTrailingHeaders);

        using var inst = new InstrumentationLog();
        TestStep testStep = null!;

        try
        {
            // Perform test step validation.
            bool isValid = _isValidFunc(responseHeaders);

            if (isValid)
            {
                testStep = CreateSuccessTestStep(inst.StartTime, inst.ElapsedTime, new PropertyBag());
            }
            else
            {
                testStep = CreateFailedTestStep(_errorMessage ?? Errors.ValidationFailed(handler: this));
            }
        }
        catch (Exception exception)
        {
            testStep = CreateTestStepFromException(exception, inst.StartTime, inst.ElapsedTime);
        }

        return Task.FromResult(testStep);
    }
}
