using XPing365.Sdk.Shared;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// This abstract class is designed to execute an action or validate test operation that is defined by the derived 
/// class.
/// </summary>
public abstract class TestComponent : ITestComponent
{
    protected TestComponent(string name, TestStepType type, IEnumerable<Type>? dataContractSerializationTypes = null)
    {
        Name = name.RequireNotNullOrEmpty(nameof(name));
        Type = type;

        if (dataContractSerializationTypes != null)
        {
            TestAgent.DataContractSerializationKnownTypes.AddRange(dataContractSerializationTypes);
        }
    }

    /// <summary>
    /// Gets a step name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a step type.
    /// </summary>
    public TestStepType Type { get; }

    /// <summary>
    /// This method performs the test step operation asynchronously. It is an abstract method that must be implemented 
    /// by the subclass.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="session">A <see cref="TestContext"/> object that represents the test session.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    public abstract Task HandleAsync(
        Uri url,
        TestSettings settings,
        TestContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously performs a probe test on the specified URL using the specified settings.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// this operation.</param>
    /// <returns><c>true</c> if the test succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is only meant for quick test validation and is stateless.
    /// </remarks>
    public virtual async Task<bool> ProbeAsync(
        Uri url,
        TestSettings settings,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        var context = new TestContext(
            sessionBuilder: new TestSessionBuilder(),
            progress: null);

        // Execute test operation by invoking the HandleAsync method of this class.
        await HandleAsync(url, settings, context, serviceProvider, cancellationToken).ConfigureAwait(false);

        return !context.SessionBuilder.HasFailed;
    }

    /// <summary>
    /// Adds a new instance of the TestComponent class to the current object.
    /// </summary>
    /// <param name="component">The TestComponent instance to add.</param>
    public void AddComponent(ITestComponent component) => GetComposite()?.AddComponent(component);

    /// <summary>
    /// Removes the specified instance of the TestComponent class from the current object.
    /// </summary>
    /// <param name="component">The TestComponent instance to remove.</param>
    public void RemoveComponent(ITestComponent component) => GetComposite()?.RemoveComponent(component);

    /// <summary>
    /// Gets a read-only collection of the child TestComponent instances of the current object.
    /// </summary>
    public IReadOnlyCollection<ITestComponent> Components => GetComposite()?.Components ?? Array.Empty<TestComponent>();

    internal virtual ICompositeTests? GetComposite() => null;
}
