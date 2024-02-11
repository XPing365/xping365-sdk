using System.Runtime.Serialization;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Core.Session;


/// <summary>
/// The ITestSessionBuilder interface is used to build test sessions. It provides methods to initialize the test session 
/// builder with the specified URL and start date, get a value indicating whether the test session has failed, get the 
/// property bag that stores key-value pairs of items that can be referenced later in the pipeline, and build a test 
/// step with the specified component, instrumentation log, and error or exception.
/// </summary>
public interface ITestSessionBuilder
{
    /// <summary>
    /// Initializes the test session builder with the specified URL and start date.
    /// </summary>
    /// <param name="url">The URL to be used for the test session.</param>
    /// <param name="startDate">The start date of the test session.</param>
    /// <returns>The initialized test session builder.</returns>
    ITestSessionBuilder Initiate(Uri url, DateTime startDate);

    /// <summary>
    /// Gets a value indicating whether the test session has failed.
    /// </summary>
    bool HasFailed { get; }

    /// <summary>
    /// Gets the property bag that stores key-value pairs of items that can be referenced later in the pipeline.
    /// </summary>
    PropertyBag<ISerializable> PropertyBag { get; }

    /// <summary>
    /// Builds a test session that has been declined by the <see cref="TestAgent"/>. 
    /// </summary>
    /// <param name="agent">A test agent object wich declined test session.</param>
    /// <param name="error">The error to be used for the test session as decline reason.</param>
    void Build(TestAgent agent, Error error);

    /// <summary>
    /// Builds a test session property bag with the speicified <see cref="PropertyBagKey"/> and 
    /// <see cref="ISerializable"/> derived type as a property bag value. 
    /// </summary>
    /// <param name="key">The property bag key that identifies the test session data.</param>
    /// <param name="value">The property bag value that contains the test session data.</param>
    /// <returns>An instance of the current ITestSessionBuilder that can be used to build the test session.</returns>
    ITestSessionBuilder Build(PropertyBagKey key, ISerializable  value);

    /// <summary>
    /// Builds a test step with the specified component and instrumentation log.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    TestStep Build(ITestComponent component, InstrumentationLog instrumentation);

    /// <summary>
    /// Builds a test step with the specified component, instrumentation log, and error.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <param name="error">The error to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    TestStep Build(ITestComponent component, InstrumentationLog instrumentation, Error error);

    /// <summary>
    /// Builds a test step with the specified component, instrumentation log, and exception.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <param name="exception">The exception to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    TestStep Build(ITestComponent component, InstrumentationLog instrumentation, Exception exception);

    /// <summary>
    /// Gets the test session.
    /// </summary>
    /// <returns>The test session.</returns>
    TestSession GetTestSession();
}

