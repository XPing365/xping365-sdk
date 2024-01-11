namespace XPing365.Sdk.Core.Validators;

/// <summary>
/// The Validator class is responsible for executing a collection of TestStepHandler objects that are used to validate 
/// TestSession object. To use the Validator class, you can create a new instance of the class passing in the concrete 
/// instances of the TestStepHandler class e.g. HttpStatusCodeValidator and provide such Validator object to
/// TestAgent.RunAsync method.
/// </summary>
/// <param name="validators">A collection of validation test steps to validate TestSession object.</param>
public sealed class Validator(params TestStepHandler[] validators) : IValidator
{
    private readonly TestStepHandler[] _validators = validators ?? [];

    /// <summary>
    /// Returns a list of validation test steps.
    /// </summary>
    public IReadOnlyCollection<TestStepHandler> Validators => _validators;

    /// <summary>
    /// Validates TestSession object by executing a collection of validators.
    /// </summary>
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
