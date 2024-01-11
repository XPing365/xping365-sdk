using System.ComponentModel.DataAnnotations;

namespace XPing365.Sdk.Core;

public enum TestStepResult
{
    [Display(Name = "succeeded")] Succeeded,
    [Display(Name = "failed")] Failed,
}
