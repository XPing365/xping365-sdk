using System.ComponentModel.DataAnnotations;

namespace XPing365.Sdk.Core.Components;

/// <summary>
/// Represents a test step result.
/// </summary>
public enum TestStepResult
{
    /// <summary>
    /// Represents a successful test result.
    /// </summary>
    [Display(Name = "succeeded")] Succeeded,

    /// <summary>
    /// Represents a failed test result.
    /// </summary>
    [Display(Name = "failed")] Failed,
}
