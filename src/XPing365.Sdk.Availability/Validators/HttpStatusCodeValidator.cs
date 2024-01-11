using System.Net;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validators;

public sealed class HttpStatusCodeValidator(
    Func<HttpStatusCode, bool> isValidFunc,
    string? errorMessage = null) : TestStepHandler(StepName, TestStepType.ValidateStep)
{
    public const string StepName = "Http status code validation";

    private readonly Func<HttpStatusCode, bool> _isValidFunc = isValidFunc;
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

        TestStep? httpRequestStep = session.Steps.FirstOrDefault(step => step.Name == SendHttpRequest.StepName);

        if (httpRequestStep == null)
        {
            return Task.FromResult(CreateFailedTestStep(Errors.InsufficientData(handler: this)));
        }

        HttpStatusCode statusCode = 
            httpRequestStep.PropertyBag.GetProperty<HttpStatusCode>(PropertyBagKeys.HttpStatus);

        using var inst = new InstrumentationLog();
        TestStep testStep = null!;

        try
        {
            // Perform test step validation.
            bool isValid = _isValidFunc(statusCode);

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
