using System.ComponentModel.DataAnnotations;

namespace XPing365.Sdk.Core.Components.Session;

/// <summary>
/// Represents the state of the <see cref="TestContext"/>.
/// </summary>
public enum TestSessionState
{
    /// <summary>
    /// The session is still being created.
    /// </summary>
    [Display(Name = "not started")] NotStarted,
    /// <summary>
    /// The session has been completed. 
    /// </summary>
    [Display(Name = "completed")] Completed,
    /// <summary>
    /// The session has been declined by test agent.
    /// </summary>
    [Display(Name = "declined")] Declined,
}
