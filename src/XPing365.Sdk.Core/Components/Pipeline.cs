namespace XPing365.Sdk.Core.Components;

/// <summary>
/// The Pipeline class is a concrete implementation of the <see cref="CompositeTests"/> class that is designed to run 
/// the test components that have been added.
/// </summary>
public class Pipeline : CompositeTests
{
    public const string StepName = nameof(Pipeline);

    public Pipeline(
        string? name = null,
        params TestComponent[] components) : base(name ?? StepName)
    {
        if (components != null)
        {
            foreach (var component in components)
            {
                AddComponent(component);
            }
        }
    }

    /// <summary>
    /// This method is designed to perform the test components that have been included in the current object.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestContext"/> object that represents the test session.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel this operation.
    /// </param>
    public override async Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var component in Components)
        {
            await component
                .HandleAsync(url, settings, context, serviceProvider, cancellationToken)
                .ConfigureAwait(false);

            // If the 'ContinueOnFailure' property is set to false and the test context contains a session that has
            // failed, then break the loop.
            if (!settings.ContinueOnFailure && context.SessionBuilder.HasFailed)
            {
                break;
            }
        }
    }
}
