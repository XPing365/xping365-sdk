namespace XPing365.Core;

public enum TestSessionState
{
    /// <summary>
    /// The session is still being created.
    /// </summary>
    NotStarted,
    /// <summary>
    /// The session has been completed. 
    /// </summary>
    Completed,
    /// <summary>
    /// The session has been declined by test agent, e.g. due to misconfiguration.
    /// </summary>
    Declined,
}
