﻿using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// The TestContext class is responsible for maintaining the state of the test execution. It encapsulates the test 
/// session builder, test component, instrumentation log, and progress reporter.
/// </summary>
public class TestContext
{
    /// <summary>
    /// Initializes new instance of the TestContext class.
    /// </summary>
    /// <param name="sessionBuilder">The session builder used to create test sessions.</param>
    /// <param name="instrumentation">
    /// The instrumentation timer associated with the current context to record test execution details.
    /// </param>
    /// <param name="progress">Optional progress reporter for tracking test execution progress.</param>
    public TestContext(
        ITestSessionBuilder sessionBuilder,
        IInstrumentation instrumentation,
        IProgress<TestStep>? progress = null)
    {
        SessionBuilder = sessionBuilder.RequireNotNull(nameof(sessionBuilder));
        Instrumentation = instrumentation.RequireNotNull(nameof(instrumentation));
        Progress = progress;
    }

    /// <summary>
    /// Gets an instance of the `ITestSessionBuilder` interface that is used to build test sessions.
    /// </summary>
    public ITestSessionBuilder SessionBuilder { get; }

    /// <summary>
    /// Gets the instrumentation timer associated with the current context to record test execution details.
    /// </summary>
    public IInstrumentation Instrumentation { get; }

    /// <summary>
    /// Gets an optional object that can be used to report progress updates for the current operation.
    /// </summary>
    public IProgress<TestStep>? Progress { get; }

    /// <summary>
    /// Gets the test component associated with the current context that executes an action or validate test operation.
    /// </summary>
    public ITestComponent? CurrentComponent { get; private set; }

    /// <summary>
    /// Updates the TestContext with the currently executing TestComponent and resets the instrumentation timer.
    /// This method should be called before executing a new TestComponent to ensure accurate timing and state tracking.
    /// </summary>
    /// <param name="newComponent">The new TestComponent that is about to be executed.</param>
    public void UpdateExecutionContext(ITestComponent newComponent)
    {
        // Update the currently executing TestComponent
        this.CurrentComponent = newComponent.RequireNotNull(nameof(newComponent));

        // Reset the instrumentation timer to measure the execution time of the new TestComponent
        this.Instrumentation.Restart();
    }
}
