using Microsoft.Extensions.DependencyInjection;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core;

/// <summary>
/// The TestAgent class is the main class that performs the testing logic of the XPing365 SDK. It runs test components, 
/// for example action or validation steps, such as DnsLookup, IPAddressAccessibilityCheck etc., using the HTTP client 
/// and the headless browser. It also creates a test session object that summarizes the outcome of the test operations.
/// </summary>
/// <remarks>
/// The TestAgent class performs the core testing logic of the XPing365 SDK. It has two methods that can execute test 
/// components that have been added to a container: <see cref="RunAsync(Uri, TestSettings, CancellationToken)"/> and 
/// <see cref="ProbeAsync(Uri, TestSettings, CancellationToken)"/>. The former executes the test components and 
/// creates a test session that summarizes the test operations. The latter serves as a quick check to ensure that the 
/// test components are properly configured and do not cause any errors. The TestAgent class collects various data 
/// related to the test execution. It constructs a <see cref="TestSession"/> object that represents the outcome of the 
/// test operations, which can be serialized, analyzed, or compared. The TestAgent class can be configured with various 
/// settings, such as the timeout, the retry, and the http headers, using the <see cref="TestSettings"/> class.
/// <para>
/// <note type="important">
/// Please note that the TestAgent class is designed to be used with a dependency injection system and should not be 
/// instantiated by the user directly. Instead, the user should register the TestAgent class in the dependency injection 
/// container using one of the supported methods, such as the 
/// <see cref="DependencyInjection.DependencyInjectionExtension.AddTestAgent(IServiceCollection)"/> extension method for 
/// the IServiceCollection interface. This way, the TestAgent class can be resolved and injected into other classes that 
/// depend on it.
/// </note>
/// <example>
/// <code>
/// Host.CreateDefaultBuilder(args)
///     .ConfigureServices((services) =>
///     {
///         services.AddHttpClients();
///         services.AddTestAgent(
///             name: "TestAgent", builder: (TestAgent agent) =>
///             {
///                 agent.Container = new Pipeline(
///                    name: "Availability pipeline",
///                    components: [
///                        new DnsLookup(),
///                        new IPAddressAccessibilityCheck(),
///                        new HttpClientRequestSender()]
///                 );
///                 return agent;
///             });
///     });
/// </code>
/// </example>
/// </para>
/// </remarks>
/// <param name="serviceProvider">An instance object of a mechanism for retrieving a service object.</param>
/// <param name="component"><see cref="ITestComponent"/> object which will be used to perform specific test operation.
/// </param>
public sealed class TestAgent(IServiceProvider serviceProvider, ITestComponent? component = null)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Gets the <see cref="ITestComponent"/> instance that represents the container of the current object.
    /// </summary>
    public ITestComponent? Container { get; set; } = component;

    /// <summary>
    /// This method initializes the test context for executing the test component. After the test operation is executed, 
    /// it constructs a test session that represents the outcome of the tests operations.
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

        try
        {
            // Initiate test session by recording its start time and the URL of the page being validated.
            context.SessionBuilder.Initiate(url, DateTime.UtcNow);

            if (Container != null)
            {
                // Execute test operation by invoking the HandleAsync method of the Container class.
                await Container
                    .HandleAsync(url, settings, context, _serviceProvider, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            context.SessionBuilder.Build(agent: this, error: Errors.ExceptionError(ex));
        }

        TestSession testSession = context.SessionBuilder.GetTestSession();
        return testSession;
    }

    public async Task<bool> ProbeAsync(
        Uri url,
        TestSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            if (Container != null)
            {
                // Execute test operation by invoking the ProbeAsync method of the Container class.
                return await Container
                    .ProbeAsync(url, settings, _serviceProvider, cancellationToken)
                    .ConfigureAwait(false);
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
