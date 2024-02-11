using XPing365.Sdk.Shared;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Core.Common;

public static class Errors
{
    public static Error ExceptionError(Exception ex) =>
        new("1000", $"Message: {ex.RequireNotNull(nameof(ex)).Message}");

    public static Error HttpClientsNotFound =>
        new("1010", $"The service provider does not have any Http clients registered. You need to invoke " +
            $"`AddHttpClients()` to add them before you can use them.");

    public static Error HeadlessBrowserNotFound =>
        new("1011", $"The service provider does not have any Headless browsers registered. You need to invoke " +
            $"`AddBrowserClient()` to add them before you can use them.");

    public static Error InsufficientData(TestComponent component) =>
        new("1100", $"Insufficient data to perform \"{component.RequireNotNull(nameof(component)).Name}\" test step.");

    public static Error ValidationFailed(TestComponent component) =>
        new("1101", $"Validation failed to perform \"{component.RequireNotNull(nameof(component)).Name}\" test step.");

    public static Error ValidationFailed(TestComponent component, string? errorMessage) =>
        new("1101", $"Validation failed to perform \"{component.RequireNotNull(nameof(component)).Name}\" test step." +
                    $" {errorMessage}");

    public static Error DnsLookupFailed =>
        new("1110", $"Could not resolve the hostname to any IP address.");

    public static Error PingRequestFailed =>
        new("1111", $"An error occurred while sending the ping request.");

    public static Error NoTestStepHandlers =>
        new("1200", $"No test step handlers were found to perform any steps. " +
                    $"The test session has been marked as declined.");

    public static Error MissingUrlInTestSession =>
        new("1201", $"Missing URL in test session");

    public static Error MissingStartTimeInTestSession =>
        new("1202", $"Missing start time in test session");

    public static Error IncorrectStartDate =>
        new("1300", $"StartDate cannot be in the past.");

}
