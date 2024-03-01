using System.Text.RegularExpressions;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestValidators;

/// <summary>
/// Represents a validator that checks the http response content against a regular expression.
/// <note>
/// The RegexContentValidator component requires the HttpRequestSender component to be registered before it in 
/// the pipeline, because it depends on the HTTP response results from the HttpRequestSender component.
/// </note>
/// </summary>
public class RegexContentValidator : BaseContentValidator
{
    public const string StepName = "Regex content validator";

    private readonly Func<Regex> GetRegex;
    private readonly Func<MatchCollection, bool> IsValid;
    private readonly string? _onError;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexContentValidator"/> class.
    /// </summary>
    /// <param name="regex">A function that returns the regular expression to use for validation.</param>
    /// <param name="isValid">A function that determines whether the match collection is valid or not.</param>
    /// <param name="onError">An optional error message to display when the validation fails.</param>
    /// <exception cref="ArgumentNullException">Thrown when the regex or isValid function is null.</exception>
    public RegexContentValidator(
        Func<Regex> regex,
        Func<MatchCollection, bool> isValid,
        string? onError = null) : base(StepName)
    {
        GetRegex = regex.RequireNotNull(nameof(regex));
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
                bool isValid = IsValid(GetRegex().Matches(content));

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
