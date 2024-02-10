using Microsoft.Extensions.DependencyInjection;
using XPing365.Sdk.Shared;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core;

/// <summary>
/// The TestAgent class is responsible for executing test components and test compositions, such as single test 
/// components aggregated in a <see cref="Pipeline"/>. It provides a base implementation for executing tests and 
/// reporting results. By subclassing the TestAgent class, test agents can focus on implementing specific test 
/// operations and leave the details of executing tests and reporting results to the base implementation.
/// </summary>
/// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
/// <param name="component"><see cref="ITestComponent"/> object which will be used to perform specific test operation.
/// </param>
public sealed class TestAgent(IServiceProvider serviceProvider, ITestComponent component)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Gets the <see cref="ITestComponent"/> instance that represents the container of the current object.
    /// </summary>
    public ITestComponent Container { get; } = component.RequireNotNull(nameof(component));

    internal static List<Type> DataContractSerializationKnownTypes { get; } = [];

    /// <summary>
    /// This method initializes the test context for executing the test component. After the test operation is executed, 
    /// it constructs a test session that represents the outcome of the operation.
    /// </summary>
    /// <param name="url">A Uri object that represents the URL of the page being validated.</param>
    /// <param name="settings">A <see cref="TestSettings"/> object that contains the settings for the test.</param>
    /// <param name="cancellationToken">An optional CancellationToken object that can be used to cancel the 
    /// validation process.</param>
    /// <returns>
    /// Returns a Task&lt;TestStession&gt; object that represents the asynchronous outcome of testing operation.
    /// </returns>
    public async Task<TestSession> RunAsync(
        Uri url,
        TestSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(settings);

        var context = new TestContext(
            sessionBuilder: _serviceProvider.GetRequiredService<ITestSessionBuilder>(),
            progress: _serviceProvider.GetService<IProgress<TestStep>>());

        // Initiate test session by recording its start time and the URL of the page being validated.
        context.SessionBuilder.Initiate(url, DateTime.UtcNow);

        try
        {
            // Execute test operation by invoking the HandleAsync method of the Container class.
            await Container.HandleAsync(url, settings, context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            context.SessionBuilder.Build(agent: this, error: Errors.ExceptionError(ex));
        }

        TestSession testSession = context.SessionBuilder.GetTestSession();
        return testSession;
    }
}
