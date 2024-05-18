using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Shared;
using XPing365.Sdk.Availability.Validations.HttpResponse.Internals;

namespace XPing365.Sdk.Availability.Validations.HttpResponse;

/// <summary>
/// Represents a validator for HTTP response. This class provides a mechanism to assert the validity of HTTP response 
/// based on user-defined criteria.
/// </summary>
public class HttpResponseValidator : TestComponent
{
    // Private field to hold the validation logic.
    private readonly Action<IHttpResponse> _validation;

    /// <summary>
    /// The name of the test component that uniquely identifies a HttpResponseValidator test operation.
    /// </summary>
    public const string StepName = nameof(HttpResponseValidator);

    /// <summary>
    /// Initializes a new instance of the HttpResponseValidator class with a specified validation action.
    /// </summary>
    /// <param name="validation">
    /// An Action delegate that encapsulates the validation logic for the HTTP response.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when the validation action is null.</exception>
    public HttpResponseValidator(Action<IHttpResponse> validation) : base(StepName, TestStepType.ValidateStep)
    {
        _validation = validation.RequireNotNull(nameof(validation));
    }

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

        using var instrumentation = new InstrumentationTimer();
        TestStep? testStep = null;

        try
        {
            var responseMessage = context.GetNonSerializablePropertyBagValue<HttpResponseMessage>(
                PropertyBagKeys.HttpResponseMessage);

            if (responseMessage == null)
            {
                testStep = context.SessionBuilder.Build(error: Errors.InsufficientData(component: this));
            }
            else
            {
                // Perform HTTP headers validation.
                _validation.Invoke(new HttpResponseInfo(responseMessage, context));

                // During HTTP headers validation, test steps are typically generated to reflect the validation process.
                // Validation is executed by user-defined code. If no steps have been created, it implies that no
                // specific content was identified for validation. In scenarios where no validation steps are generated,
                // a successful test step is added to denote the completion of the test operation.
                // The presence of an empty property bag signifies that no specific validation was executed.
                if (context.SessionBuilder.Steps.Count == 0)
                {
                    testStep = context.SessionBuilder.Build();
                }
            }
        }
        catch (ValidationException ex)
        {
            testStep = context.SessionBuilder.Build(Errors.ValidationFailed(component: this, errorMessage: ex.Message));
        }
        catch (Exception exception)
        {
            testStep = context.SessionBuilder.Build(exception);
        }
        finally
        {
            if (testStep != null)
            {
                context.Progress?.Report(testStep);
            }
        }

        return Task.CompletedTask;
    }
}
