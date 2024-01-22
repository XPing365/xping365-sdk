namespace XPing365.Sdk.Core.Validators;

/// <summary>
/// Defines the properties and methods that objects that participate in <see cref="TestSession"/> 
/// validation must implement.
/// </summary>
/// <remarks>
/// The IValidator interface is implemented by the <see cref="Validator"/> class, which is a helper class aggregating 
/// collection of <see cref="TestStepHandler"/> objects that are used to validate <see cref="TestSession"/> instance.
/// </remarks>
public interface IValidator
{
    /// <summary>
    /// Returns a read-only collection of validation test steps.
    /// </summary>
    IReadOnlyCollection<TestStepHandler> Validators { get; }

    /// <summary>
    /// This method is used to validate a <see cref="TestSession"/> object
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestSession"/> object that represents the test session.</param>
    /// <param name="progress">An optional IProgress&lt;TestStep?&gt; object that can be used to report progress during
    /// the validation process.
    /// </param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// validation process.
    /// </param>
    /// <returns>Returns a Task object that represents the asynchronous validation operation.</returns>
    Task ValidateAsync(
        Uri url,
        TestSettings settings,
        TestSession session,
        IProgress<TestStep>? progress = null,
        CancellationToken cancellationToken = default);
}
