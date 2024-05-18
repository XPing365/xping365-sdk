using Microsoft.Playwright;
using XPing365.Sdk.Availability.Validations.Content.Html.Internals;
using XPing365.Sdk.Availability.Validations.Content.Html.Internals.Selectors;
using XPing365.Sdk.Availability.Validations.Content.Page.Internals;
using XPing365.Sdk.Core.Clients.Browser;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Extensions;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.Validations.Content.Page;

/// <summary>
/// Represents a validator that checks if a http response content matches a specified pattern or condition.
/// </summary>
/// <remarks>
/// <note>
/// The PageContentValidator component requires the Browser component to be registered before it in the pipeline, 
/// because it depends on the HTTP response results from this component.
/// </note>
/// </remarks>
public class PageContentValidator : BaseContentValidator
{
    private readonly Action<IPage> _validation;

    /// <summary>
    /// The name of the test component that represents a StringContentValidator test operation.
    /// </summary>
    /// <remarks>
    /// This constant is used to register the StringContentValidator class in the test framework.
    /// </remarks>
    public const string StepName = nameof(PageContentValidator);

    /// <summary>
    /// Initializes a new instance of the <see cref="PageContentValidator"/> class.
    /// </summary>
    /// <param name="validation">
    /// A function that determines whether the html content matches a specififed condition.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when the validation function is null.</exception>
    public PageContentValidator(Action<IPage> validation) : base(StepName)
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

        TestStep? testStep = null;

        try
        {
            var response = context
                .GetNonSerializablePropertyBagValue<BrowserResponseMessage>(PropertyBagKeys.HttpResponseMessage);

            if (response == null)
            {
                testStep = context.SessionBuilder.Build(error: Errors.InsufficientData(component: this));
            }
            else
            {
                IPage page = response.Page;

                // Perform Page validation.
                _validation(new InstrumentedPage(context, page));

                // During Page validation, test steps are typically generated to reflect the validation process.
                // Validation is executed by user-defined code. If no steps have been created, it implies that no
                // specific content was identified for validation. In scenarios where no validation steps are
                // generated, a successful test step is added to denote the completion of the test operation.
                // The presence of an empty property bag signifies that no specific validation was executed.
                if (context.SessionBuilder.Steps.Count == 0)
                {
                    testStep = context.SessionBuilder.Build();
                }
            }
        }
        catch (ValidationException ex)
        {
            testStep = context.SessionBuilder.Build(
                Errors.ValidationFailed(component: this, errorMessage: ex.Message));
        }
        catch (Exception ex)
        {
            testStep = context.SessionBuilder.Build(ex);
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

