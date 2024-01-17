using System.ComponentModel.DataAnnotations;

namespace XPing365.Sdk.Core;

/// <summary>
/// The TestStepType enum is used to specify the type of <see cref="TestStepHandler"/>, whether it is an action step or 
/// a validation step. An action step is used to create an action for instance retrieve data, while a validation step is 
/// used to validate retrieved data for its correctness. 
/// </summary>
public enum TestStepType
{
    /// <summary>
    /// Represents action step.
    /// </summary>
    [Display(Name = "action step")] ActionStep = 0,
    /// <summary>
    /// Represents validate step.
    /// </summary>
    [Display(Name = "validate step")] ValidateStep = 1
}
