using System.ComponentModel.DataAnnotations;

namespace XPing365.Sdk.Core;

public enum TestStepType
{
    [Display(Name = "action step")] ActionStep = 0,
    [Display(Name = "validate step")] ValidateStep = 1
}
