using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestValidators;

/// <summary>
/// Represents a validator that checks if a http response content matches a specified pattern or condition.
/// <note>
/// The StringContentValidator component requires the HttpRequestSender component to be registered before it in 
/// the pipeline, because it depends on the HTTP response results from the HttpRequestSender component.
/// </note>
/// </summary>
public class StringContentValidator : BaseContentValidator
{
    /// <summary>
    /// The name of the test component that represents a StringContentValidator test operation.
    /// </summary>
    /// <remarks>
    /// This constant is used to register the StringContentValidator class in the test framework.
    /// </remarks>
    public const string StepName = "String content validator";

    private readonly Func<string, bool> _isValid;
    private readonly string? _onError;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringContentValidator"/> class.
    /// </summary>
    /// <param name="isValid">A function that determines whether the content matches a specififed condition.</param>
    /// <param name="onError">An optional error message to display when the validation fails.</param>
    /// <exception cref="ArgumentNullException">Thrown when the isValid function is null.</exception>
    public StringContentValidator(Func<string, bool> isValid, string? onError = null) : base(StepName)
    {
        _isValid = isValid.RequireNotNull(nameof(isValid));
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

                // Perform test step validation.
                bool isValid = _isValid(content);

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
