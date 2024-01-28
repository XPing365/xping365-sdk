using XPing365.Sdk.Common;
using XPing365.Sdk.Core.Components.Session;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// The TestContext class is responsible for maintaining the state of the test execution.
/// </summary>
/// <param name="sessionBuilder"></param>
/// <param name="progress"></param>
public class TestContext(ITestSessionBuilder sessionBuilder, IProgress<TestStep>? progress)
{
    /// <summary>
    /// Gets an instance of the `ITestSessionBuilder` interface that is used to build test sessions.
    /// </summary>
    public ITestSessionBuilder SessionBuilder { get; } = sessionBuilder.RequireNotNull(nameof(sessionBuilder));

    /// <summary>
    /// Gets an object that can be used to report progress updates for the current operation.
    /// </summary>
    public IProgress<TestStep>? Progress { get; } = progress;
}
