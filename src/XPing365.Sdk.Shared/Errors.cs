using XPing365.Sdk.Core;

namespace XPing365.Sdk.Shared;

internal static class Errors
{
    #region General errors
    public static Error ExceptionError(Exception ex) =>
        new("1000", $"Exception of type {ex.GetType()} occured. Message: {ex.Message}.");
    #endregion
    #region TestStepHandler and derived class errors
    public static Error InsufficientData(TestStepHandler handler) =>
        new("1100", $"Insufficient data to perform \"{handler.Name}\" test step.");
    public static Error ValidationFailed(TestStepHandler handler) =>
        new("1101", $"Validation failed to perform \"{handler.Name}\" test step.");
    public static Error DnsLookupFailed =>
        new("1102", $"Could not resolve the hostname to any IP address.");
    #endregion
    #region TestAgent errors
    public static Error NoTestStepHandlers =>
        new("1200", $"No test step handlers were found to perform any steps. " +
                    $"The test session has been marked as declined.");
    #endregion
    #region TestStep errors
    public static Error IncorrectStartDate => 
        new("1300", $"StartDate cannot be in the past.");
    #endregion
}
