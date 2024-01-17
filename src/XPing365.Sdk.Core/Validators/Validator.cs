namespace XPing365.Sdk.Core.Validators;

/// <summary>
/// The Validator class is responsible for executing a collection of <see cref="TestStepHandler"/> objects that are 
/// used to validate <see cref="TestSession"/> object. To use the Validator class, you can create a new instance of the 
/// class passing in the concrete instances of the <see cref="TestStepHandler"/> class for instance 
/// HttpStatusCodeValidator and provide such Validator object to 
/// <see cref="TestAgent.RunAsync(Uri, TestSettings, XPing365.Sdk.Core.Validators.IValidator?, IProgress{XPing365.Sdk.Core.TestStep}?, CancellationToken)"/>
/// method.
/// </summary>
/// <param name="validators">A collection of validation test steps to validate <see cref="TestSession"/> object.</param>
public sealed class Validator(params TestStepHandler[] validators) : IValidator
{
    private readonly TestStepHandler[] _validators = validators ?? [];

    /// <summary>
    /// Returns a read-only collection of validation test steps.
    /// </summary>
    public IReadOnlyCollection<TestStepHandler> Validators => _validators;

    /// <summary>
    /// This method is used to validate a <see cref="TestSession"/> object.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="progress">An optional IProgress&lt;TestStep&gt; object that can be used to report progress during 
    /// the validation process.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// validation process.</param>
    /// <returns>Returns a Task object that represents the asynchronous validation operation.</returns>
    /// <remarks>
    /// This method does nothing if no validators have been provided.
    /// </remarks>
    /// <exception cref="ArgumentNullException">If any of the following parameters: url, settings or session is null.
    /// </exception>
    public async Task ValidateAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        IProgress<TestStep>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(session);

        foreach (var validator in _validators)
        {
            TestStep testStep = await validator.HandleStepAsync(
                url,
                settings,
                session,
                cancellationToken).ConfigureAwait(false);

            if (testStep != null)
            {
                session.AddTestStep(testStep);
                progress?.Report(testStep);
            }
        }
    }
}
