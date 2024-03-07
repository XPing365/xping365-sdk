using XPing365.Sdk.Shared;
using XPing365.Sdk.Core.Session;
using Microsoft.Extensions.DependencyInjection;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// This abstract class is designed to execute an action or validate test operation that is defined by the derived 
/// class.
/// </summary>
public abstract class TestComponent : ITestComponent
{
    /// <summary>
    /// Initiazlies new instance of the TestComponent class.
    /// </summary>
    /// <param name="name">Name of the test component.</param>
    /// <param name="type">Type of the test component.</param>
    protected TestComponent(string name, TestStepType type)
    {
        Name = name.RequireNotNullOrEmpty(nameof(name));
        Type = type;
    }

    /// <summary>
    /// Gets a step name.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets a test component type.
    /// </summary>
    public TestStepType Type { get; }

    /// <summary>
    /// This method performs the test step operation asynchronously. It is an abstract method that must be implemented 
    /// by the subclass.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="context">A <see cref="TestContext"/> object that represents the test context.</param>
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
            sessionBuilder: serviceProvider.GetRequiredService<ITestSessionBuilder>(),
            progress: serviceProvider.GetService<IProgress<TestStep>>());

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
    /// <returns>
    /// <c>true</c> if component is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c>
    /// when component was not found.
    /// </returns>
    public bool RemoveComponent(ITestComponent component) => GetComposite()?.RemoveComponent(component) ?? false;

    /// <summary>
    /// Gets a read-only collection of the child TestComponent instances of the current object.
    /// </summary>
    public IReadOnlyCollection<ITestComponent> Components => GetComposite()?.Components ?? Array.Empty<TestComponent>();

    internal virtual ICompositeTests? GetComposite() => null;
}
