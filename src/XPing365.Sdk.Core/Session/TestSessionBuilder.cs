﻿using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Session;

/// <summary>
/// TestSessionBuilder is a concrete implementation of the <see cref="ITestSessionBuilder"/> interface that is used to 
/// build test sessions.
/// </summary>
public class TestSessionBuilder : ITestSessionBuilder
{
    private readonly List<TestStep> _steps = [];
    private readonly Dictionary<ITestComponent, int> _componentStepCounts = [];

    private DateTime _startDate = DateTime.MinValue;
    private Error? _error;
    private Uri? _url;
    private PropertyBag<IPropertyBagValue>? _propertyBag;

    /// <summary>
    /// Gets a value indicating whether the test session has failed.
    /// </summary>
    public bool HasFailed => _steps.Any(step => step.Result == TestStepResult.Failed);

    /// <summary>
    /// Gets a collection of test steps associated with the current instance of the test session builder.
    /// </summary>
    public ReadOnlyCollection<TestStep> Steps => _steps.AsReadOnly();

    /// <summary>
    /// Initializes the test session builder with the specified URL and start date.
    /// </summary>
    /// <param name="url">The URL to be used for the test session.</param>
    /// <param name="startDate">The start date of the test session.</param>
    /// <returns>The initialized test session builder.</returns>
    public ITestSessionBuilder Initiate(Uri url, DateTime startDate)
    {
        _startDate = startDate;
        _url = url;

        return this;
    }

    /// <summary>
    /// Builds a test session that has been declined by the <see cref="TestAgent"/>. 
    /// </summary>
    /// <param name="agent">A test agent object wich declined test session.</param>
    /// <param name="error">The error to be used for the test session as decline reason.</param>
    public void Build(TestAgent agent, Error error)
    {
        _error = error.RequireNotNull(nameof(error));
    }

    /// <summary>
    /// Builds a test session property bag with the speicified <see cref="PropertyBagKey"/> and 
    /// <see cref="ISerializable"/> derived type as a property bag value. 
    /// </summary>
    /// <param name="key">The property bag key that identifies the test session data.</param>
    /// <param name="value">The property bag value that contains the test session data.</param>
    /// <returns>An instance of the current ITestSessionBuilder that can be used to build the test session.</returns>
    public ITestSessionBuilder Build(PropertyBagKey key, IPropertyBagValue value)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        _propertyBag ??= new();
        _propertyBag.AddOrUpdateProperty(key, value);

        return this;
    }

    /// <summary>
    /// Builds a test step with the specified component and instrumentation log. Test step is marked as succeeded.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    public TestStep Build(ITestComponent component, InstrumentationLog instrumentation)
    {
        ArgumentNullException.ThrowIfNull(component, nameof(component));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));

        // Increment the step count for the given ITestComponent instance to ensure each component step is accurately
        // named based on its number of steps within the test session.
        _componentStepCounts[component] = _componentStepCounts.TryGetValue(component, out int count) ? count + 1 : 1;

        var testStep = new TestStep
        {
            Name = component.Name,
            TestComponentIteration = _componentStepCounts[component],
            StartDate = instrumentation.StartTime,
            Duration = instrumentation.ElapsedTime,
            Type = component.Type,
            PropertyBag = _propertyBag,
            Result = TestStepResult.Succeeded,
            ErrorMessage = null
        };

        _steps.Add(testStep);

        // Set property bag to null to force its reconstruction when new data is added.
        _propertyBag = null;

        return testStep;
    }

    /// <summary>
    /// Builds a test step with the specified component, instrumentation log, and error. Test step is marked as failed.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <param name="error">The error to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    public TestStep Build(ITestComponent component, InstrumentationLog instrumentation, Error error)
    {
        ArgumentNullException.ThrowIfNull(component, nameof(component));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));
        ArgumentNullException.ThrowIfNull(error, nameof(error));

        // Increment the step count for the given ITestComponent instance to ensure each component step is accurately
        // named based on its number of steps within the test session.
        _componentStepCounts[component] = _componentStepCounts.TryGetValue(component, out int count) ? count + 1 : 1;

        var testStep = new TestStep
        {
            Name = component.Name,
            TestComponentIteration = _componentStepCounts[component],
            StartDate = instrumentation.StartTime,
            Duration = instrumentation.ElapsedTime,
            Type = component.Type,
            PropertyBag = _propertyBag,
            Result = TestStepResult.Failed,
            ErrorMessage = error
        };

        _steps.Add(testStep);

        // Set property bag to null to force its reconstruction when new data is added.
        _propertyBag = null;

        return testStep;
    }

    /// <summary>
    /// Builds a test step with the specified component, instrumentation log, and exception. Test step is marked as
    /// failed.
    /// </summary>
    /// <param name="component">The component to be used for the test step.</param>
    /// <param name="instrumentation">The instrumentation log to be used for the test step.</param>
    /// <param name="exception">The exception to be used for the test step.</param>
    /// <returns>The built test step.</returns>
    public TestStep Build(ITestComponent component, InstrumentationLog instrumentation, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(component, nameof(component));
        ArgumentNullException.ThrowIfNull(instrumentation, nameof(instrumentation));
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        // Increment the step count for the given ITestComponent instance to ensure each component step is accurately
        // named based on its number of steps within the test session.
        _componentStepCounts[component] = _componentStepCounts.TryGetValue(component, out int count) ? count + 1 : 1;

        var testStep = new TestStep
        {
            Name = component.Name,
            TestComponentIteration = _componentStepCounts[component],
            StartDate = instrumentation.StartTime,
            Duration = instrumentation.ElapsedTime,
            Type = component.Type,
            PropertyBag = _propertyBag,
            Result = TestStepResult.Failed,
            ErrorMessage = Errors.ExceptionError(exception)
        };

        _steps.Add(testStep);

        // Set property bag to null to force its reconstruction when new data is added.
        _propertyBag = null;

        return testStep;
    }

    /// <summary>
    /// Gets the test session.
    /// </summary>
    /// <returns>The test session.</returns>
    public TestSession GetTestSession()
    {
        try
        {
            if (_error != null)
            {
                throw new ArgumentException(_error.Message);
            }

            var session = new TestSession
            {
                Url = _url!,
                StartDate = _startDate,
                Steps = _steps,
                // Set the test session status to completed to indicate that no further modifications are allowed.
                State = TestSessionState.Completed,
                DeclineReason = null
            };

            return session;
        }
        catch (Exception ex)
        {
            var session = new TestSession
            {
                Url = _url ?? new Uri("/", UriKind.Relative),
                StartDate = _startDate < DateTime.UtcNow ? DateTime.UtcNow : _startDate,
                Steps = _steps,
                State = TestSessionState.Declined,
                DeclineReason = ex.Message
            };

            return session;
        }
    }
}
