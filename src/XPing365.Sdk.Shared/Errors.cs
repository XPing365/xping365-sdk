using XPing365.Sdk.Core;

namespace XPing365.Sdk.Shared;

internal static class Errors
{
    public static Error ExceptionError(Exception ex) => 
        new("1000", $"Exception of type {ex.GetType()} occured. Message: {ex.Message}.");

    public static Error InsufficientData(TestStepHandler handler) => 
        new("1100", $"Insufficient data to perform \"{handler.Name}\" test step.");
    public static Error ValidationFailed(TestStepHandler handler) =>
        new("1110", $"Validation failed to perform \"{handler.Name}\" test step.");
    public static Error NoTestStepHandlers =>
        new("1120", $"No test step handlers were found to perform any steps. " +
                    $"The test session has been marked as declined.");
}
