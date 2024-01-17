using System.Net;
using XPing365.Sdk.Availability.TestSteps;
using XPing365.Sdk.Core;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validators;

/// <summary>
/// The HttpStatusCodeValidator class is a concrete implementation of the <see cref="TestStepHandler"/> class that is 
/// used to validate the HTTP status code of an HTTP response. It takes a Func&lt;HttpStatusCode, bool&gt; delegate as a
/// parameter, which is used to validate the HTTP status code. The errorMessage parameter is an optional error message 
/// that can be used to provide additional information about the validation failure.
/// </summary>
/// <example>
/// <code>
/// var statusCodeValidator = new HttpStatusCodeValidator(
///     isValid: (HttpStatusCode code) => code == HttpStatusCode.OK, 
///     errorMessage: (HttpStatusCode code) => $"The HTTP request failed with status code {code}"
/// );
/// var validator = new Validator(statusCodeValidator);
/// </code>
/// </example>
/// <param name="isValid">Func&lt;HttpStatusCode, bool&gt; delegate to validate the HTTP status code.</param>
/// <param name="errorMessage">Optional information about the validation failure.</param>
public sealed class HttpStatusCodeValidator(
    Func<HttpStatusCode, bool> isValid,
    Func<HttpStatusCode, string>? errorMessage = null) : TestStepHandler(StepName, TestStepType.ValidateStep)
{
    public const string StepName = "Http status code validation";

    private readonly Func<HttpStatusCode, bool> _isValid = isValid;
    private readonly Func<HttpStatusCode, string>? _errorMessage = errorMessage;

    /// <summary>
    /// This method performs the test step operation asynchronously.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel this operation.
    /// </param>
    /// <returns><see cref="TestStep"/> object.</returns>
    public override Task<TestStep> HandleStepAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
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
            bool isValid = _isValid(statusCode);

            if (isValid)
            {
                testStep = CreateSuccessTestStep(inst.StartTime, inst.ElapsedTime, new PropertyBag());
            }
            else
            {
                string? errmsg = _errorMessage?.Invoke(statusCode);
                testStep = CreateFailedTestStep(errmsg ?? Errors.ValidationFailed(handler: this));
            }
        }
        catch (Exception exception)
        {
            testStep = CreateTestStepFromException(exception, inst.StartTime, inst.ElapsedTime);
        }
        
        return Task.FromResult(testStep);
    }
}
